using Medical_atention.Constants;
using Medical_atention.Models;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Medical_atention.Services
{
    public class AuthService : IAuthService
    {
        private static readonly HttpClient _client = new HttpClient { Timeout = System.TimeSpan.FromSeconds(10) };

        public async Task<LoginResponse> LoginAsync(string email, string password)
        {
            var payload = JsonConvert.SerializeObject(new { email, password });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync(AppConstants.ApiBaseUrl + "/api/auth/login", content);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return null;

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<LoginResponse>(json);
        }

        public async Task<bool> ChangePasswordAsync(string token, string currentPassword, string newPassword)
        {
            var payload = JsonConvert.SerializeObject(new { currentPassword, newPassword });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Put, AppConstants.ApiBaseUrl + "/api/auth/password")
            {
                Content = content
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return false;

            response.EnsureSuccessStatusCode();
            return true;
        }
    }
}
