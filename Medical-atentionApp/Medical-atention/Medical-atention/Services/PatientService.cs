using Medical_atention.Data;
using Medical_atention.Helpers;
using Medical_atention.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Medical_atention.Services
{
    public class PatientService : IPatientService
    {
        private static readonly HttpMethod PatchMethod = new HttpMethod("PATCH");
        private readonly PatientRepository _repository = new PatientRepository();

        // ── Conectividad ──────────────────────────────────────────────────────

        public bool IsOnline() =>
            Connectivity.NetworkAccess == NetworkAccess.Internet;

        // ── Listado ───────────────────────────────────────────────────────────

        public async Task<IReadOnlyList<Patient>> LoadPatientsAsync(bool forceRefresh = false)
        {
            if (IsOnline())
            {
                try
                {
                    var remote = await FetchPatientsFromApiAsync("api/patients");
                    if (remote.Count > 0)
                    {
                        await _repository.ReplaceAllAsync(remote);
                        return remote;
                    }
                }
                catch (Exception)
                {
                    if (!forceRefresh)
                        return await _repository.GetAllAsync();
                    throw;
                }
            }

            return await _repository.GetAllAsync();
        }

        public async Task<Patient> GetPatientAsync(int id)
        {
            var local = await _repository.GetByIdAsync(id);
            if (local != null) return local;

            var all = await LoadPatientsAsync();
            return all.FirstOrDefault(p => p.Id == id);
        }

        // ── Registro ──────────────────────────────────────────────────────────

        public async Task<(Patient patient, string error)> RegisterAsync(PatientRequestDto request)
        {
            try
            {
                using (var client = await ApiClient.CreateAsync())
                {
                    var content = new StringContent(
                        JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

                    var response = await client.PostAsync("api/patients", content);

                    if (response.StatusCode == HttpStatusCode.Conflict)
                        return (null, "Ya existe un paciente con esta cédula");

                    if (!response.IsSuccessStatusCode)
                        return (null, "Error al registrar el paciente");

                    var dto = JsonConvert.DeserializeObject<PatientResponseDto>(
                        await response.Content.ReadAsStringAsync());

                    var patient = MapToPatient(dto);
                    await _repository.UpsertAsync(patient);
                    return (patient, null);
                }
            }
            catch (Exception)
            {
                return (null, "Sin conexión, verifica tu red");
            }
        }

        // ── Triaje ────────────────────────────────────────────────────────────

        public async Task<(IReadOnlyList<Patient> patients, bool fromCache, string error)> GetPatientsByPriorityAsync()
        {
            if (IsOnline())
            {
                try
                {
                    var remote = await FetchPatientsFromApiAsync("api/patients?sort=priority");
                    if (remote.Count > 0)
                    {
                        await _repository.ReplaceAllAsync(remote);
                        await SecureStorage.SetAsync(
                            Constants.AppConstants.LastPatientSyncKey,
                            DateTime.UtcNow.ToString("o"));
                        return (remote, false, null);
                    }
                }
                catch { }
            }

            var cached = await _repository.GetAllSortedByPriorityAsync();
            if (cached.Count > 0)
                return (cached, true, null);

            return (new List<Patient>(), true, "Sin conexión y sin datos locales");
        }

        public async Task<(bool success, string error)> UpdatePriorityAsync(int id, PriorityLevel priority)
        {
            if (IsOnline())
            {
                try
                {
                    using (var client = await ApiClient.CreateAsync())
                    {
                        var content = new StringContent(
                            JsonConvert.SerializeObject(new UpdatePatientPriorityRequest { Priority = priority }),
                            Encoding.UTF8,
                            "application/json");

                        var req = new HttpRequestMessage(PatchMethod, $"api/patients/{id}/priority")
                        {
                            Content = content
                        };
                        var response = await client.SendAsync(req);

                        if (response.IsSuccessStatusCode)
                        {
                            var dto = JsonConvert.DeserializeObject<PatientResponseDto>(
                                await response.Content.ReadAsStringAsync());
                            await _repository.ClearPendingPriorityAsync(id, dto.Priority);
                            await _repository.UpsertAsync(MapToPatient(dto));
                            return (true, null);
                        }

                        return (false, "No se pudo actualizar la prioridad");
                    }
                }
                catch
                {
                    return (false, "Error de red al actualizar prioridad");
                }
            }

            await _repository.SetPendingPriorityAsync(id, priority);
            return (true, null);
        }

        public async Task SyncPendingPriorityChangesAsync()
        {
            if (!IsOnline()) return;

            var pending = await _repository.GetPendingPriorityUpdatesAsync();
            foreach (var patient in pending)
            {
                if (!patient.PendingPriority.HasValue) continue;
                var priority = (PriorityLevel)patient.PendingPriority.Value;
                var (success, _) = await UpdatePriorityAsync(patient.Id, priority);
                if (success)
                    await _repository.ClearPendingPriorityAsync(patient.Id, priority);
            }
        }

        // ── Mapeo DTO → dominio (responsabilidad exclusiva de la capa de servicio) ──

        private static Patient MapToPatient(PatientResponseDto dto) =>
            new Patient
            {
                Id = dto.Id,
                FirstName = dto.Name ?? string.Empty,
                LastName = dto.LastName ?? string.Empty,
                DocumentNumber = dto.IdentificationNumber ?? string.Empty,
                Priority = (int)dto.Priority,
                LastConsultationAt = dto.LastConsultationAt,
                PendingPrioritySync = false,
                PendingPriority = null
            };

        private static async Task<List<Patient>> FetchPatientsFromApiAsync(string path)
        {
            using (var client = await ApiClient.CreateAsync())
            {
                var response = await client.GetAsync(path);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var dtos = JsonConvert.DeserializeObject<List<PatientResponseDto>>(json)
                    ?? new List<PatientResponseDto>();

                return dtos.Select(MapToPatient).ToList();
            }
        }
    }
}
