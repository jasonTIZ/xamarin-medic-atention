using Medical_atention.Data;
using Medical_atention.Helpers;
using Medical_atention.Models;
using Medical_atention.Models.Entities;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Medical_atention.Services
{
    public class ConsultationService : IConsultationService
    {
        private readonly IPatientRepository _patientRepository = new PatientRepository();
        private readonly IConsultationRepository _consultationRepository = new ConsultationRepository();
        private readonly ISyncQueueRepository _syncQueueRepository = new SyncQueueRepository();
        public async Task<(bool success, string error)> RegisterConsultationAsync(int patientId)
        {
            var patient = await _patientRepository.GetByIdAsync(patientId);
            if (patient == null)
                return (false, "Paciente no encontrado");

            var now = DateTime.UtcNow;
            var consultation = new ConsultationEntity
            {
                LocalId = Guid.NewGuid(),
                PatientId = patient.Id,
                PatientLocalId = patient.LocalId,
                ConsultationDate = now,
                CreatedAt = now,
                Notes = "Consulta registrada desde triaje",
                PendingSync = true
            };

            patient.LastConsultationAt = now;

            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    using (var client = await ApiClient.CreateAsync())
                    {
                        var request = new ConsultationRequestDto
                        {
                            PatientId = patient.Id,
                            ConsultationDate = now,
                            Notes = consultation.Notes
                        };
                        var content = new System.Net.Http.StringContent(
                            JsonConvert.SerializeObject(request),
                            System.Text.Encoding.UTF8,
                            "application/json");
                        var response = await client.PostAsync("api/consultations", content);

                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync();
                            var dto = JsonConvert.DeserializeObject<ConsultationResponseDto>(json);
                            consultation.Id = dto.Id;
                            consultation.PendingSync = false;
                            patient.LastConsultationAt = dto.ConsultationDate;
                            await _consultationRepository.InsertAsync(consultation);
                            await _patientRepository.UpsertAsync(patient);
                            return (true, null);
                        }
                    }
                }
                catch
                {
                    // Continúa con guardado offline.
                }
            }

            await _consultationRepository.InsertAsync(consultation);
            patient.PendingSync = true;
            await _patientRepository.UpsertAsync(patient);

            await _syncQueueRepository.EnqueueAsync(new SyncQueueEntity
            {
                EntityType = SyncEntityType.Consultation,
                EntityLocalId = consultation.LocalId,
                Operation = SyncOperation.CreateConsultation,
                PayloadJson = JsonConvert.SerializeObject(new ConsultationRequestDto
                {
                    PatientId = patient.Id,
                    ConsultationDate = now,
                    Notes = consultation.Notes
                })
            });

            return (true, "Consulta guardada localmente; se sincronizará al reconectar");
        }
    }
}
