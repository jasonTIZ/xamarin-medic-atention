using Medical_atention.Data;
using Medical_atention.Helpers;
using Medical_atention.Models;
using Medical_atention.Models.Entities;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Medical_atention.Services
{
    public class SyncService : ISyncService
    {
        private static readonly HttpMethod PatchMethod = new HttpMethod("PATCH");
        private readonly ISyncQueueRepository _queueRepository = new SyncQueueRepository();
        private readonly IPatientRepository _patientRepository = new PatientRepository();
        private readonly IConsultationRepository _consultationRepository = new ConsultationRepository();

        public async Task<int> SyncPendingAsync()
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                return 0;

            var synced = 0;
            var queue = await _queueRepository.GetPendingOrderedAsync();

            foreach (var item in queue)
            {
                try
                {
                    var success = await ProcessQueueItemAsync(item);
                    if (success)
                    {
                        await _queueRepository.RemoveAsync(item.Id);
                        synced++;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[SyncService] Error en {item.Operation} ({item.EntityLocalId}): {ex.Message}");
                }
            }

            if (synced > 0)
            {
                LocalDataChangedHelper.NotifyPatientsChanged();
                SyncNotificationHelper.NotifyCompleted(synced);
            }

            return synced;
        }

        private async Task<bool> ProcessQueueItemAsync(SyncQueueEntity item)
        {
            switch (item.Operation)
            {
                case SyncOperation.UpdatePatientPriority:
                    return await SyncPatientPriorityAsync(item);
                case SyncOperation.CreatePatient:
                    return await SyncCreatePatientAsync(item);
                case SyncOperation.CreateConsultation:
                    return await SyncCreateConsultationAsync(item);
                default:
                    Debug.WriteLine($"[SyncService] Operación desconocida: {item.Operation}");
                    return false;
            }
        }

        private async Task<bool> SyncPatientPriorityAsync(SyncQueueEntity item)
        {
            var payload = JsonConvert.DeserializeObject<UpdatePatientPriorityRequest>(item.PayloadJson);
            var patient = await _patientRepository.GetByLocalIdAsync(item.EntityLocalId);
            if (patient == null || patient.Id <= 0) return false;

            using (var client = await ApiClient.CreateAsync())
            {
                var content = new StringContent(
                    JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                var request = new HttpRequestMessage(PatchMethod, $"api/patients/{patient.Id}/priority")
                {
                    Content = content
                };
                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode) return false;

                var json = await response.Content.ReadAsStringAsync();
                var dto = JsonConvert.DeserializeObject<PatientResponseDto>(json);
                var entity = PatientMapper.ToEntity(MapToPatient(dto));
                entity.LocalId = patient.LocalId;
                entity.PendingSync = false;
                entity.PendingPrioritySync = false;
                entity.PendingPriority = null;
                await _patientRepository.UpsertAsync(entity);
                return true;
            }
        }

        private async Task<bool> SyncCreatePatientAsync(SyncQueueEntity item)
        {
            var request = JsonConvert.DeserializeObject<PatientRequestDto>(item.PayloadJson);
            var local = await _patientRepository.GetByLocalIdAsync(item.EntityLocalId);
            if (local == null) return false;

            using (var client = await ApiClient.CreateAsync())
            {
                var content = new StringContent(
                    JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("api/patients", content);
                if (!response.IsSuccessStatusCode) return false;

                var json = await response.Content.ReadAsStringAsync();
                var dto = JsonConvert.DeserializeObject<PatientResponseDto>(json);
                var entity = PatientMapper.ToEntity(MapToPatient(dto));
                entity.LocalId = local.LocalId;
                entity.PendingSync = false;
                entity.PendingPrioritySync = false;
                await _patientRepository.UpsertAsync(entity);
                return true;
            }
        }

        private async Task<bool> SyncCreateConsultationAsync(SyncQueueEntity item)
        {
            var request = JsonConvert.DeserializeObject<ConsultationRequestDto>(item.PayloadJson);
            var local = await _consultationRepository.GetByLocalIdAsync(item.EntityLocalId);
            if (local == null) return false;

            if (local.PatientId <= 0)
            {
                var patient = await _patientRepository.GetByLocalIdAsync(local.PatientLocalId);
                if (patient == null || patient.Id <= 0) return false;
                request.PatientId = patient.Id;
            }

            using (var client = await ApiClient.CreateAsync())
            {
                var content = new StringContent(
                    JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("api/consultations", content);
                if (!response.IsSuccessStatusCode) return false;

                var json = await response.Content.ReadAsStringAsync();
                var dto = JsonConvert.DeserializeObject<ConsultationResponseDto>(json);

                local.Id = dto.Id;
                local.PatientId = dto.PatientId;
                local.PendingSync = false;
                await _consultationRepository.UpdateAsync(local);

                var patientEntity = await _patientRepository.GetByIdAsync(dto.PatientId);
                if (patientEntity != null)
                {
                    patientEntity.LastConsultationAt = dto.ConsultationDate;
                    await _patientRepository.UpsertAsync(patientEntity);
                }

                return true;
            }
        }

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
                PendingPrioritySync = false
            };
    }
}
