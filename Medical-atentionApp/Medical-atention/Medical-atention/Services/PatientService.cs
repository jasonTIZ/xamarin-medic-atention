using Medical_atention.Constants;
using Medical_atention.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

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

        private static HttpRequestMessage BuildRequest(HttpMethod method, string path, string token)
        {
            var request = new HttpRequestMessage(method, AppConstants.ApiBaseUrl + path);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return request;
        }
    }
}
