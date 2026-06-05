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

        public async Task<Patient> GetPatientAsync(int id)
        {
            var local = await _repository.GetByIdAsync(id);
            if (local != null) return PatientMapper.ToDomain(local);

            var all = await LoadPatientsAsync();
            return all.FirstOrDefault(p => p.Id == id);
        }

        public async Task<(Patient patient, string error)> RegisterAsync(PatientRequestDto request)
        {
            if (IsOnline())
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
                        await _repository.UpsertAsync(PatientMapper.ToEntity(patient));
                        return (patient, null);
                    }
                }
                catch (Exception)
                {
                    // Continúa con registro offline.
                }
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
                            Constants.AppConstants.LastPatientSyncKey,
                            DateTime.UtcNow.ToString("o"));
                        return (remote, false, null);
                    }
                }
                catch { }
            }

            var cached = (await _repository.GetAllSortedByPriorityAsync())
                .Select(PatientMapper.ToDomain)
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
                            await _repository.ClearPendingPriorityAsync(id, dto.Priority);
                            var entity = PatientMapper.ToEntity(MapToPatient(dto));
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

        private static Patient MapToPatient(PatientResponseDto dto) =>
            new Patient
            {
                Id = dto.Id,
                FirstName = dto.Name ?? string.Empty,
                LastName = dto.LastName ?? string.Empty,
                DocumentNumber = dto.IdentificationNumber ?? string.Empty,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender ?? string.Empty,
                Priority = (int)dto.Priority,
                LastConsultationAt = dto.LastConsultationAt,
                PendingSync = false,
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
