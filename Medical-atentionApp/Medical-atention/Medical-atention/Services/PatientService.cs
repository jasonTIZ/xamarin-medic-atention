using Medical_atention.Constants;
using Medical_atention.Data;
using Medical_atention.Helpers;
using Medical_atention.Models;
using Medical_atention.Models.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Medical_atention.Services
{
    public class PatientService : IPatientService
    {
        private static readonly HttpMethod PatchMethod = new HttpMethod("PATCH");
        private readonly IPatientRepository _repository = new PatientRepository();
        private readonly ISyncQueueRepository _syncQueueRepository = new SyncQueueRepository();
        private readonly ISyncService _syncService = new SyncService();

        public bool IsOnline() =>
            Connectivity.NetworkAccess == NetworkAccess.Internet;

        // ── CRUD con token (detalle paciente — dev) ─────────────────────────────

        public async Task<(PatientResponseDto patient, string error)> RegisterAsync(
            PatientRequestDto request, string token)
        {
            try
            {
                using (var client = await CreateClientAsync(token))
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
                    await _repository.UpsertAsync(PatientMapper.FromDtoToEntity(dto));
                    return (dto, null);
                }
            }
            catch (Exception)
            {
                return (null, "Sin conexión, verifica tu red");
            }
        }

        public async Task<PatientResponseDto> GetPatientAsync(int id, string token)
        {
            if (!IsOnline())
            {
                var offline = await _repository.GetByIdAsync(id);
                return offline == null ? null : MapEntityToDto(offline);
            }

            try
            {
                using (var client = await CreateClientAsync(token))
                {
                    var response = await client.GetAsync($"api/patients/{id}");
                    if (!response.IsSuccessStatusCode)
                    {
                        var fallback = await _repository.GetByIdAsync(id);
                        return fallback == null ? null : MapEntityToDto(fallback);
                    }

                    var dto = JsonConvert.DeserializeObject<PatientResponseDto>(
                        await response.Content.ReadAsStringAsync());
                    if (dto != null)
                        await _repository.UpsertAsync(PatientMapper.FromDtoToEntity(dto));
                    return dto;
                }
            }
            catch (Exception)
            {
                var local = await _repository.GetByIdAsync(id);
                return local == null ? null : MapEntityToDto(local);
            }
        }

        public async Task<List<PatientResponseDto>> GetAllPatientsAsync(string token)
        {
            if (!IsOnline())
                return await GetAllPatientsFromLocalAsync();

            try
            {
                using (var client = await CreateClientAsync(token))
                {
                    var response = await client.GetAsync("api/patients");
                    if (!response.IsSuccessStatusCode)
                        return await GetAllPatientsFromLocalAsync();

                    var patients = JsonConvert.DeserializeObject<List<PatientResponseDto>>(
                                       await response.Content.ReadAsStringAsync())
                                   ?? new List<PatientResponseDto>();

                    if (patients.Count > 0)
                    {
                        await _repository.ReplaceAllAsync(patients.Select(PatientMapper.FromDtoToEntity));
                        LocalDataChangedHelper.NotifyPatientsChanged();
                    }

                    return patients;
                }
            }
            catch (Exception)
            {
                return await GetAllPatientsFromLocalAsync();
            }
        }

        public async Task<(PatientResponseDto patient, string error)> UpdateAsync(
            int id, PatientRequestDto request, string token)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                return (null, "Paciente no encontrado");

            if (!IsOnline())
                return await UpdatePatientLocallyAsync(entity, request);

            try
            {
                using (var client = await CreateClientAsync(token))
                {
                    var content = new StringContent(
                        JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                    var response = await client.PutAsync($"api/patients/{id}", content);

                    if (response.StatusCode == HttpStatusCode.Conflict)
                        return (null, "Ya existe un paciente con esta cédula");

                    if (!response.IsSuccessStatusCode)
                        return (null, "Error al guardar los cambios");

                    var dto = JsonConvert.DeserializeObject<PatientResponseDto>(
                        await response.Content.ReadAsStringAsync());
                    await _repository.UpsertAsync(PatientMapper.FromDtoToEntity(dto));
                    LocalDataChangedHelper.NotifyPatientsChanged();
                    return (dto, null);
                }
            }
            catch (Exception)
            {
                return await UpdatePatientLocallyAsync(entity, request);
            }
        }

        public async Task<bool> DeleteAsync(int id, string token)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                return false;

            if (!IsOnline())
                return await DeletePatientLocallyAsync(entity);

            try
            {
                using (var client = await CreateClientAsync(token))
                {
                    var response = await client.DeleteAsync($"api/patients/{id}");
                    if (response.IsSuccessStatusCode)
                    {
                        await _repository.DeleteAsync(id);
                        LocalDataChangedHelper.NotifyPatientsChanged();
                        return true;
                    }

                    return false;
                }
            }
            catch (Exception)
            {
                return await DeletePatientLocallyAsync(entity);
            }
        }

        // ── Triaje / offline ──────────────────────────────────────────────────

        public async Task<IReadOnlyList<Patient>> LoadPatientsFromLocalAsync()
        {
            var entities = await _repository.GetAllAsync();
            return entities.Select(PatientMapper.ToDomain).ToList();
        }

        public async Task<IReadOnlyList<Patient>> LoadPatientsAsync(bool forceRefresh = false)
        {
            if (!IsOnline())
                return await LoadPatientsFromLocalAsync();

            try
            {
                await _syncService.SyncPendingAsync();
                var remote = await FetchPatientsFromApiAsync("api/patients");
                if (remote.Count > 0)
                {
                    await _repository.ReplaceAllAsync(remote.Select(PatientMapper.ToEntity));
                    LocalDataChangedHelper.NotifyPatientsChanged();
                    return remote;
                }
            }
            catch (Exception)
            {
                if (!forceRefresh)
                    return await LoadPatientsFromLocalAsync();
                throw;
            }

            return await LoadPatientsFromLocalAsync();
        }

        public async Task<(Patient patient, string error)> RegisterLocalAsync(PatientRequestDto request)
        {
            var token = await SecureStorage.GetAsync(AppConstants.TokenKey);
            if (IsOnline() && !string.IsNullOrEmpty(token))
            {
                var (dto, error) = await RegisterAsync(request, token);
                if (dto != null)
                    return (PatientMapper.FromDto(dto), null);
                if (error != null && !error.Contains("conexión"))
                    return (null, error);
            }

            var localPatient = new Patient
            {
                Id = 0,
                LocalId = Guid.NewGuid(),
                PendingSync = true,
                FirstName = request.Name?.Trim() ?? string.Empty,
                LastName = request.LastName?.Trim() ?? string.Empty,
                DocumentNumber = request.IdentificationNumber?.Trim() ?? string.Empty,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender ?? string.Empty,
                Priority = (int)PriorityLevel.Medium
            };

            await _repository.UpsertAsync(PatientMapper.ToEntity(localPatient));
            await _syncQueueRepository.EnqueueAsync(new SyncQueueEntity
            {
                EntityType = SyncEntityType.Patient,
                EntityLocalId = localPatient.LocalId,
                Operation = SyncOperation.CreatePatient,
                PayloadJson = JsonConvert.SerializeObject(request)
            });

            return (localPatient, null);
        }

        public async Task<(IReadOnlyList<Patient> patients, bool fromCache, string error)> GetPatientsByPriorityAsync()
        {
            if (IsOnline())
            {
                try
                {
                    await _syncService.SyncPendingAsync();
                    var remote = await FetchPatientsFromApiAsync("api/patients?sort=priority");
                    if (remote.Count > 0)
                    {
                        await _repository.ReplaceAllAsync(remote.Select(PatientMapper.ToEntity));
                        await SecureStorage.SetAsync(
                            AppConstants.LastPatientSyncKey,
                            DateTime.UtcNow.ToString("o"));
                        return (remote, false, null);
                    }
                }
                catch { }
            }

            var cached = (await _repository.GetAllSortedByPriorityAsync())
                .Select(PatientMapper.ToDomain)
                .ToList();

            await ApplyConsultationPrioritiesAsync(cached);
            cached = cached
                .OrderBy(p => p.EffectivePriority)
                .ThenBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .ToList();

            if (cached.Count > 0)
                return (cached, true, null);

            return (new List<Patient>(), true, "Sin conexión y sin datos locales");
        }

        public async Task<(bool success, string error)> UpdatePriorityAsync(int id, PriorityLevel priority)
        {
            var patientEntity = await _repository.GetByIdAsync(id);
            if (patientEntity == null)
                return (false, "Paciente no encontrado");

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
                            await _repository.ClearPendingPriorityAsync(id, priority);
                            var entity = PatientMapper.FromDtoToEntity(dto);
                            entity.LocalId = patientEntity.LocalId;
                            await _repository.UpsertAsync(entity);
                            LocalDataChangedHelper.NotifyPatientsChanged();
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
            await _syncQueueRepository.RemoveByEntityLocalIdAsync(
                patientEntity.LocalId, SyncOperation.UpdatePatientPriority);
            await _syncQueueRepository.EnqueueAsync(new SyncQueueEntity
            {
                EntityType = SyncEntityType.Patient,
                EntityLocalId = patientEntity.LocalId,
                Operation = SyncOperation.UpdatePatientPriority,
                PayloadJson = JsonConvert.SerializeObject(new UpdatePatientPriorityRequest { Priority = priority })
            });

            LocalDataChangedHelper.NotifyPatientsChanged();
            return (true, null);
        }

        public Task SyncPendingPriorityChangesAsync() =>
            _syncService.SyncPendingAsync();

        // ── Helpers ─────────────────────────────────────────────────────────────

        private static async Task<HttpClient> CreateClientAsync(string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri(AppConstants.ApiBaseUrl.TrimEnd('/') + "/"),
                    Timeout = TimeSpan.FromSeconds(15)
                };
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                return client;
            }

            return await ApiClient.CreateAsync();
        }

        private static PatientResponseDto MapEntityToDto(PatientEntity entity)
        {
            var domain = PatientMapper.ToDomain(entity);
            return new PatientResponseDto
            {
                Id = domain.Id,
                Name = domain.FirstName,
                LastName = domain.LastName,
                IdentificationNumber = domain.DocumentNumber,
                DateOfBirth = domain.DateOfBirth,
                Gender = domain.Gender,
                CreatedAt = DateTime.UtcNow,
                Priority = PatientMapper.ToPriorityString(domain.EffectivePriority),
                LastConsultationDate = domain.LastConsultationAt
            };
        }

        private async Task<List<PatientResponseDto>> GetAllPatientsFromLocalAsync()
        {
            return (await _repository.GetAllAsync())
                .Select(MapEntityToDto)
                .ToList();
        }

        private async Task<(PatientResponseDto patient, string error)> UpdatePatientLocallyAsync(
            PatientEntity entity, PatientRequestDto request)
        {
            entity.FirstName = request.Name?.Trim() ?? string.Empty;
            entity.LastName = request.LastName?.Trim() ?? string.Empty;
            entity.DocumentNumber = request.IdentificationNumber?.Trim() ?? string.Empty;
            entity.DateOfBirth = request.DateOfBirth;
            entity.Gender = request.Gender ?? string.Empty;
            entity.PendingSync = true;
            await _repository.UpsertAsync(entity);

            await _syncQueueRepository.RemoveByEntityLocalIdAsync(
                entity.LocalId, SyncOperation.UpdatePatient);
            await _syncQueueRepository.EnqueueAsync(new SyncQueueEntity
            {
                EntityType = SyncEntityType.Patient,
                EntityLocalId = entity.LocalId,
                Operation = SyncOperation.UpdatePatient,
                PayloadJson = JsonConvert.SerializeObject(request)
            });

            LocalDataChangedHelper.NotifyPatientsChanged();
            return (MapEntityToDto(entity), null);
        }

        private async Task<bool> DeletePatientLocallyAsync(PatientEntity entity)
        {
            if (entity.Id <= 0)
            {
                await _syncQueueRepository.RemoveByEntityLocalIdAsync(
                    entity.LocalId, SyncOperation.CreatePatient);
                await _repository.DeleteByLocalIdAsync(entity.LocalId);
                LocalDataChangedHelper.NotifyPatientsChanged();
                return true;
            }

            await _repository.DeleteAsync(entity.Id);
            await _syncQueueRepository.EnqueueAsync(new SyncQueueEntity
            {
                EntityType = SyncEntityType.Patient,
                EntityLocalId = entity.LocalId,
                Operation = SyncOperation.DeletePatient,
                PayloadJson = JsonConvert.SerializeObject(new { PatientId = entity.Id })
            });
            LocalDataChangedHelper.NotifyPatientsChanged();
            return true;
        }

        private static async Task ApplyConsultationPrioritiesAsync(List<Patient> patients)
        {
            var localPriorities = await LocalDatabase.Instance.GetLatestPrioritiesByPatientAsync();
            foreach (var patient in patients)
            {
                if (!localPriorities.TryGetValue(patient.Id, out var local))
                    continue;

                if (patient.LastConsultationAt == null || local.ConsultationDate > patient.LastConsultationAt)
                {
                    patient.Priority = PatientMapper.ParsePriority(local.Priority);
                    patient.LastConsultationAt = local.ConsultationDate;
                }
            }
        }

        private static async Task<List<Patient>> FetchPatientsFromApiAsync(string path)
        {
            using (var client = await ApiClient.CreateAsync())
            {
                var response = await client.GetAsync(path);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var dtos = JsonConvert.DeserializeObject<List<PatientResponseDto>>(json)
                    ?? new List<PatientResponseDto>();

                return dtos.Select(PatientMapper.FromDto).ToList();
            }
        }
    }
}
