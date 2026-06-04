using Medical_atention.Constants;
using Medical_atention.Data;
using Medical_atention.Models;
using Newtonsoft.Json;
using System;
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

        public ConsultationService() : this(LocalDatabase.Instance) { }

        public ConsultationService(LocalDatabase localDb)
        {
            _localDb = localDb;
        }

        public async Task<RegisterConsultationResult> RegisterAsync(ConsultationRequestDto request, string token)
        {
            var isOnline = Connectivity.NetworkAccess == NetworkAccess.Internet;

            if (isOnline)
            {
                try
                {
                    var message = BuildRequest(HttpMethod.Post, "/api/consultations", token);
                    message.Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                    var response = await _client.SendAsync(message);

                    if (response.StatusCode == HttpStatusCode.Created)
                    {
                        var consultation = JsonConvert.DeserializeObject<ConsultationResponseDto>(
                            await response.Content.ReadAsStringAsync());

                        await _localDb.SaveConsultationAsync(ToLocal(consultation, pendingSync: false));
                        return new RegisterConsultationResult
                        {
                            Success = true,
                            SavedOffline = false,
                            Consultation = consultation
                        };
                    }

                    if (response.StatusCode == HttpStatusCode.BadRequest)
                        return new RegisterConsultationResult { Error = "Datos inválidos. Revisa los campos." };
                }
                catch (Exception) when (isOnline) { }
            }

            await _localDb.SaveConsultationAsync(ToLocal(request, pendingSync: true));
            return new RegisterConsultationResult
            {
                Success = true,
                SavedOffline = true
            };
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

        private static HttpRequestMessage BuildRequest(HttpMethod method, string path, string token)
        {
            var request = new HttpRequestMessage(method, AppConstants.ApiBaseUrl + path);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return request;
        }
    }
}
