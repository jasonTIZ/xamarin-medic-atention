using Medical_atention.Constants;
using Medical_atention.Data;
using Medical_atention.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
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
        private static readonly HttpMethod PatchMethod = new HttpMethod("PATCH");
        private static readonly HttpClient _client = new HttpClient { Timeout = System.TimeSpan.FromSeconds(10) };
        private readonly PatientRepository _repository = new PatientRepository();

        public async Task<(PatientResponseDto patient, string error)> RegisterAsync(PatientRequestDto request, string token)
        {
            var message = BuildRequest(HttpMethod.Post, "/api/patients", token);
            message.Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            var response = await _client.SendAsync(message);

            if (response.StatusCode == HttpStatusCode.Conflict)
                return (null, "Ya existe un paciente con esta cédula");

            if (!response.IsSuccessStatusCode)
                return (null, "Error al registrar el paciente");

            var patient = JsonConvert.DeserializeObject<PatientResponseDto>(await response.Content.ReadAsStringAsync());
            await _repository.UpsertAsync(patient);
            return (patient, null);
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

        public async Task<(List<PatientResponseDto> patients, bool fromCache, string error)> GetPatientsByPriorityAsync(string token)
        {
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    var response = await _client.SendAsync(
                        BuildRequest(HttpMethod.Get, "/api/patients?sort=priority", token));

                    if (response.IsSuccessStatusCode)
                    {
                        var patients = JsonConvert.DeserializeObject<List<PatientResponseDto>>(
                            await response.Content.ReadAsStringAsync());
                        await _repository.ReplaceAllAsync(patients);
                        await SecureStorage.SetAsync(
                            AppConstants.LastPatientSyncKey,
                            System.DateTime.UtcNow.ToString("o"));
                        return (patients, false, null);
                    }
                }
                catch { }
            }

            var cached = await _repository.GetAllSortedByPriorityAsync();
            if (cached.Count > 0)
                return (cached, true, null);

            return (new List<PatientResponseDto>(), true, "Sin conexión y sin datos locales");
        }

        public async Task<(bool success, string error)> UpdatePriorityAsync(int id, PriorityLevel priority, string token)
        {
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    var message = BuildRequest(PatchMethod, $"/api/patients/{id}/priority", token);
                    message.Content = new StringContent(
                        JsonConvert.SerializeObject(new UpdatePatientPriorityRequest { Priority = priority }),
                        Encoding.UTF8,
                        "application/json");

                    var response = await _client.SendAsync(message);
                    if (response.IsSuccessStatusCode)
                    {
                        var updated = JsonConvert.DeserializeObject<PatientResponseDto>(
                            await response.Content.ReadAsStringAsync());
                        await _repository.ClearPendingPriorityAsync(id, updated.Priority);
                        await _repository.UpsertAsync(updated);
                        return (true, null);
                    }

                    return (false, "No se pudo actualizar la prioridad");
                }
                catch
                {
                    return (false, "Error de red al actualizar prioridad");
                }
            }

            await _repository.SetPendingPriorityAsync(id, priority);
            return (true, null);
        }

        public async Task SyncPendingPriorityChangesAsync(string token)
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet) return;

            var pending = await _repository.GetPendingPriorityUpdatesAsync();
            foreach (var entity in pending)
            {
                if (!entity.PendingPriority.HasValue) continue;
                var priority = (PriorityLevel)entity.PendingPriority.Value;
                var result = await UpdatePriorityAsync(entity.Id, priority, token);
                if (result.success)
                    await _repository.ClearPendingPriorityAsync(entity.Id, priority);
            }
        }

        private static HttpRequestMessage BuildRequest(HttpMethod method, string path, string token)
        {
            var request = new HttpRequestMessage(method, AppConstants.ApiBaseUrl + path);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return request;
        }
    }
}
