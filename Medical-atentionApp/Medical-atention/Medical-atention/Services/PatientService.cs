using Medical_atention.Constants;
using Medical_atention.Data;
using Medical_atention.Models;
using Newtonsoft.Json;
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
    public class PatientService : IPatientService
    {
        private static readonly HttpClient _client = new HttpClient { Timeout = System.TimeSpan.FromSeconds(10) };

        public async Task<(PatientResponseDto patient, string error)> RegisterAsync(PatientRequestDto request, string token)
        {
            var message = BuildRequest(HttpMethod.Post, "/api/patients", token);
            message.Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            var response = await _client.SendAsync(message);

            if (response.StatusCode == HttpStatusCode.Conflict)
                return (null, "Ya existe un paciente con esta cédula");

            if (!response.IsSuccessStatusCode)
                return (null, "Error al registrar el paciente");

            return (JsonConvert.DeserializeObject<PatientResponseDto>(await response.Content.ReadAsStringAsync()), null);
        }

        public async Task<PatientResponseDto> GetPatientAsync(int id, string token)
        {
            var response = await _client.SendAsync(BuildRequest(HttpMethod.Get, $"/api/patients/{id}", token));
            if (!response.IsSuccessStatusCode) return null;
            return JsonConvert.DeserializeObject<PatientResponseDto>(await response.Content.ReadAsStringAsync());
        }

        public async Task<List<PatientResponseDto>> GetAllPatientsAsync(string token)
        {
            var response = await _client.SendAsync(BuildRequest(HttpMethod.Get, "/api/patients", token));
            if (!response.IsSuccessStatusCode) return new List<PatientResponseDto>();
            return JsonConvert.DeserializeObject<List<PatientResponseDto>>(await response.Content.ReadAsStringAsync());
        }

        public async Task<(PatientResponseDto patient, string error)> UpdateAsync(int id, PatientRequestDto request, string token)
        {
            var message = BuildRequest(HttpMethod.Put, $"/api/patients/{id}", token);
            message.Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            var response = await _client.SendAsync(message);

            if (response.StatusCode == HttpStatusCode.Conflict)
                return (null, "Ya existe un paciente con esta cédula");

            if (!response.IsSuccessStatusCode)
                return (null, "Error al guardar los cambios");

            return (JsonConvert.DeserializeObject<PatientResponseDto>(await response.Content.ReadAsStringAsync()), null);
        }

        public async Task<bool> DeleteAsync(int id, string token)
        {
            var response = await _client.SendAsync(BuildRequest(HttpMethod.Delete, $"/api/patients/{id}", token));
            return response.IsSuccessStatusCode;
        }

        public async Task<PatientHistorySummary> GetPatientHistoryAsync(int patientId, string token)
        {
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    var response = await _client.SendAsync(BuildRequest(HttpMethod.Get, $"/api/patients/{patientId}/history", token));
                    if (response.IsSuccessStatusCode)
                        return JsonConvert.DeserializeObject<PatientHistorySummary>(await response.Content.ReadAsStringAsync());
                }
                catch { }
            }

            var local = await LocalDatabase.Instance.GetConsultationsByPatientAsync(patientId);
            if (!local.Any()) return null;

            return new PatientHistorySummary
            {
                PatientId = patientId,
                TotalConsultations = local.Count,
                LastConsultationDate = local.First().ConsultationDate.ToString("yyyy-MM-dd"),
                CurrentPriority = local.First().Priority,
                RecentDiagnoses = local
                    .Where(c => !string.IsNullOrWhiteSpace(c.Diagnosis))
                    .Take(5)
                    .Select(c => new DiagnosisSummary { Diagnosis = c.Diagnosis, Date = c.ConsultationDate.ToString("yyyy-MM-dd") })
                    .ToList(),
                FrequentMedications = ExtractMedications(local.Select(c => c.Treatment)).ToList(),
                PriorityEvolution = local
                    .OrderBy(c => c.ConsultationDate)
                    .Select(c => new PriorityPoint { Date = c.ConsultationDate.ToString("yyyy-MM-dd"), Priority = c.Priority })
                    .ToList()
            };
        }

        private static IEnumerable<string> ExtractMedications(IEnumerable<string> treatments)
        {
            var seen = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            foreach (var treatment in treatments.Where(t => !string.IsNullOrWhiteSpace(t)))
                foreach (var segment in treatment.Split(new[] { ',', ';', '\n' }, System.StringSplitOptions.RemoveEmptyEntries))
                {
                    var entry = segment.Trim();
                    if (entry.Length >= 4 && entry.Length <= 80)
                        seen.Add(entry);
                }
            return seen.Take(10);
        }

        private static HttpRequestMessage BuildRequest(HttpMethod method, string path, string token)
        {
            var request = new HttpRequestMessage(method, AppConstants.ApiBaseUrl + path);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return request;
        }
    }
}
