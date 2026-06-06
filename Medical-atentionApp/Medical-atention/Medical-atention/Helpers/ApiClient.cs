using Medical_atention.Constants;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Medical_atention.Helpers
{
    public static class ApiClient
    {
        public static async Task<HttpClient> CreateAsync()
        {
            var baseUrl = await ApiBaseUrlResolver.ResolveAsync();
            var client = new HttpClient
            {
                BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/"),
                Timeout = TimeSpan.FromSeconds(15)
            };

            try
            {
                var token = await SecureStorage.GetAsync(AppConstants.TokenKey);
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            catch (Exception)
            {
                // SecureStorage unavailable
            }

            return client;
        }
    }
}
