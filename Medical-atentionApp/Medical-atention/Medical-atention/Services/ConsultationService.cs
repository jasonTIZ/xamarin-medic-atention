using Medical_atention.Constants;
using Medical_atention.Data;
using Medical_atention.Helpers;
using Medical_atention.Models;
using static Medical_atention.Helpers.ApiExceptionHandler;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Medical_atention.Services
{
    public class ConsultationService : IConsultationService
    {
        private static readonly HttpClient _client = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        private readonly LocalDatabase _localDb;
        private readonly IPatientRepository _patientRepository;
        private readonly INotificationService _notificationService;

        public ConsultationService()
            : this(LocalDatabase.Instance, new PatientRepository(), new NotificationService()) { }

        public ConsultationService(LocalDatabase localDb, IPatientRepository patientRepository)
            : this(localDb, patientRepository, new NotificationService()) { }

        public ConsultationService(
            LocalDatabase localDb,
            IPatientRepository patientRepository,
            INotificationService notificationService)
        {
            _localDb = localDb;
            _patientRepository = patientRepository;
            _notificationService = notificationService;
        }

        public async Task<RegisterConsultationResult> RegisterAsync(ConsultationRequestDto request, string token)
        {
            var isOnline = Connectivity.NetworkAccess == NetworkAccess.Internet;

            if (isOnline)
            {
                try
                {
                    var message = await BuildRequestAsync(HttpMethod.Post, "/api/consultations", token);
                    message.Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                    var response = await _client.SendAsync(message);

                    if (response.StatusCode == HttpStatusCode.Created)
                    {
                        var consultation = JsonConvert.DeserializeObject<ConsultationResponseDto>(
                            await response.Content.ReadAsStringAsync());

                        await _localDb.SaveConsultationAsync(ToLocal(consultation, pendingSync: false));
                        await ApplyConsultationPriorityToPatientAsync(
                            consultation.PatientId,
                            consultation.ConsultationDate,
                            consultation.Priority);
                        await TryScheduleUrgentFollowUpAsync(request);
                        return new RegisterConsultationResult
                        {
                            Success = true,
                            SavedOffline = false,
                            Consultation = consultation
                        };
                    }

                    if (response.StatusCode == HttpStatusCode.BadRequest)
                        return new RegisterConsultationResult { Error = "Datos inválidos. Revisa los campos." };

                    var apiError = await ProcessResponseAsync(response, "POST /api/consultations");
                    if (apiError != null)
                        return new RegisterConsultationResult { Error = apiError };
                }
                catch (Exception ex) when (isOnline)
                {
                    HandleException(ex, "POST /api/consultations");
                }
            }

            await _localDb.SaveConsultationAsync(ToLocal(request, pendingSync: true));
            await ApplyConsultationPriorityToPatientAsync(
                request.PatientId,
                request.ConsultationDate,
                request.Priority);
            await TryScheduleUrgentFollowUpAsync(request);
            return new RegisterConsultationResult
            {
                Success = true,
                SavedOffline = true
            };
        }

        // Recordatorio de seguimiento: solo para consultas urgentes, a 24 horas.
        // Muestra el nombre del paciente y el motivo (diagnóstico, o síntomas como respaldo).
        private async Task TryScheduleUrgentFollowUpAsync(ConsultationRequestDto request)
        {
            if (!string.Equals(request.Priority, "urgent", StringComparison.OrdinalIgnoreCase))
                return;

            try
            {
                var patient = await _patientRepository.GetByIdAsync(request.PatientId);
                var patientName = patient != null
                    ? $"{patient.FirstName} {patient.LastName}".Trim()
                    : $"Paciente #{request.PatientId}";

                var reason = !string.IsNullOrWhiteSpace(request.Diagnosis)
                    ? request.Diagnosis
                    : !string.IsNullOrWhiteSpace(request.Symptoms)
                        ? request.Symptoms
                        : "Consulta urgente";

                await _notificationService.ScheduleFollowUpAsync(
                    request.PatientId, patientName, reason, DateTime.Now.AddHours(24));
            }
            catch (Exception)
            {
                // No interrumpir el registro de la consulta si falla la programación.
            }
        }

        private async Task ApplyConsultationPriorityToPatientAsync(
            int patientId, DateTime consultationDate, string priority)
        {
            var patient = await _patientRepository.GetByIdAsync(patientId);
            if (patient == null) return;

            if (patient.LastConsultationAt.HasValue
                && patient.LastConsultationAt.Value > consultationDate)
                return;

            patient.Priority = PatientMapper.ParsePriority(priority);
            patient.LastConsultationAt = consultationDate;
            patient.PendingPrioritySync = false;
            patient.PendingPriority = null;
            await _patientRepository.UpsertAsync(patient);
            LocalDataChangedHelper.NotifyPatientsChanged();
        }

        private static LocalConsultation ToLocal(ConsultationResponseDto dto, bool pendingSync)
            => new LocalConsultation
            {
                ServerId = dto.Id,
                PatientId = dto.PatientId,
                ConsultationDate = dto.ConsultationDate,
                Symptoms = dto.Symptoms,
                Diagnosis = dto.Diagnosis,
                Treatment = dto.Treatment,
                Notes = dto.Notes,
                Priority = dto.Priority,
                PendingSync = pendingSync,
                CreatedAt = dto.CreatedAt
            };

        private static LocalConsultation ToLocal(ConsultationRequestDto request, bool pendingSync)
            => new LocalConsultation
            {
                PatientId = request.PatientId,
                ConsultationDate = request.ConsultationDate,
                Symptoms = request.Symptoms,
                Diagnosis = request.Diagnosis,
                Treatment = request.Treatment,
                Notes = request.Notes,
                Priority = request.Priority,
                PendingSync = pendingSync,
                CreatedAt = DateTime.UtcNow
            };

        public async Task<List<ConsultationListItem>> GetByPatientAsync(int patientId, DateTime from, DateTime to, string token)
        {
            var merged = new Dictionary<string, ConsultationListItem>();

            var localItems = await _localDb.GetConsultationsByPatientAsync(patientId);
            foreach (var local in FilterByDate(localItems, from, to))
                merged[GetKey(local.ServerId, local.LocalId)] = FromLocal(local);

            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    var fromStr = from.ToString("yyyy-MM-dd");
                    var toStr = to.ToString("yyyy-MM-dd");
                    var path = $"/api/consultations?patient_id={patientId}&from={fromStr}&to={toStr}";
                    var response = await _client.SendAsync(await BuildRequestAsync(HttpMethod.Get, path, token));

                    if (response.IsSuccessStatusCode)
                    {
                        var apiList = JsonConvert.DeserializeObject<List<ConsultationResponseDto>>(
                            await response.Content.ReadAsStringAsync()) ?? new List<ConsultationResponseDto>();

                        foreach (var dto in apiList)
                            merged[GetKey(dto.Id, 0)] = FromApi(dto, localItems);
                    }
                }
                catch (Exception) { }
            }

            return merged.Values
                .OrderByDescending(c => c.ConsultationDate)
                .ToList();
        }

        public async Task<(bool success, string error)> UpdateAsync(
            int? serverId, int localId, ConsultationUpdateDto request, string token)
        {
            var treatment = request.Treatment?.Trim() ?? string.Empty;
            var notes = request.Notes?.Trim() ?? string.Empty;
            var synced = false;

            if (serverId.HasValue && serverId.Value > 0 && Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    var message = await BuildRequestAsync(HttpMethod.Put, $"/api/consultations/{serverId.Value}", token);
                    message.Content = new StringContent(
                        JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

                    var response = await _client.SendAsync(message);
                    if (response.IsSuccessStatusCode)
                    {
                        synced = true;
                        var updated = JsonConvert.DeserializeObject<ConsultationResponseDto>(
                            await response.Content.ReadAsStringAsync());
                        await _localDb.UpdateConsultationTreatmentNotesAsync(
                            localId, serverId, updated.Treatment, updated.Notes, pendingSync: false);
                        return (true, null);
                    }

                    if (response.StatusCode == HttpStatusCode.NotFound)
                        return (false, "Consulta no encontrada");
                }
                catch (Exception) { }
            }

            var saved = await _localDb.UpdateConsultationTreatmentNotesAsync(
                localId, serverId, treatment, notes, pendingSync: !synced);

            return saved ? (true, null) : (false, "No se pudo guardar los cambios");
        }

        public async Task<ConsultationResponseDto> GetDetailAsync(int? serverId, int localId, string token)
        {
            if (serverId.HasValue && Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    var response = await _client.SendAsync(
                        await BuildRequestAsync(HttpMethod.Get, $"/api/consultations/{serverId.Value}", token));

                    if (response.IsSuccessStatusCode)
                        return JsonConvert.DeserializeObject<ConsultationResponseDto>(
                            await response.Content.ReadAsStringAsync());
                }
                catch (Exception) { }
            }

            LocalConsultation local = null;
            if (localId > 0)
                local = await _localDb.GetConsultationByLocalIdAsync(localId);
            else if (serverId.HasValue)
                local = await _localDb.GetConsultationByServerIdAsync(serverId.Value);

            if (local is null) return null;

            return new ConsultationResponseDto
            {
                Id = local.ServerId ?? 0,
                PatientId = local.PatientId,
                ConsultationDate = local.ConsultationDate,
                Symptoms = local.Symptoms,
                Diagnosis = local.Diagnosis,
                Treatment = local.Treatment,
                Notes = local.Notes,
                Priority = local.Priority,
                CreatedAt = local.CreatedAt
            };
        }

        private static IEnumerable<LocalConsultation> FilterByDate(
            IEnumerable<LocalConsultation> items, DateTime from, DateTime to)
        {
            var end = to.Date.AddDays(1).AddTicks(-1);
            return items.Where(c => c.ConsultationDate >= from.Date && c.ConsultationDate <= end);
        }

        private static string GetKey(int? serverId, int localId)
            => serverId.HasValue && serverId.Value > 0 ? $"s-{serverId}" : $"l-{localId}";

        private static ConsultationListItem FromLocal(LocalConsultation local)
            => new ConsultationListItem
            {
                ServerId = local.ServerId,
                LocalId = local.LocalId,
                PatientId = local.PatientId,
                ConsultationDate = local.ConsultationDate,
                Symptoms = local.Symptoms,
                Diagnosis = local.Diagnosis,
                Treatment = local.Treatment,
                Notes = local.Notes,
                Priority = local.Priority,
                PendingSync = local.PendingSync
            };

        private static ConsultationListItem FromApi(ConsultationResponseDto dto, List<LocalConsultation> localItems)
        {
            var local = localItems.FirstOrDefault(l => l.ServerId == dto.Id);
            return new ConsultationListItem
            {
                ServerId = dto.Id,
                LocalId = local?.LocalId ?? 0,
                PatientId = dto.PatientId,
                ConsultationDate = dto.ConsultationDate,
                Symptoms = dto.Symptoms,
                Diagnosis = dto.Diagnosis,
                Treatment = dto.Treatment,
                Notes = dto.Notes,
                Priority = dto.Priority,
                PendingSync = local?.PendingSync ?? false
            };
        }

        private static async Task<HttpRequestMessage> BuildRequestAsync(HttpMethod method, string path, string token)
        {
            var baseUrl = await ApiBaseUrlResolver.ResolveAsync();
            var request = new HttpRequestMessage(method, baseUrl + path);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return request;
        }
    }
}
