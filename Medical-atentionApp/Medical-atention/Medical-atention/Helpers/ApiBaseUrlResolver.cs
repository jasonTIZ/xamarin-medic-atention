using Medical_atention.Constants;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Medical_atention.Helpers
{
    public static class ApiBaseUrlResolver
    {
        private static string _cachedUrl;

        public static string CachedUrl => _cachedUrl;

        public static IReadOnlyList<string> GetCandidates()
        {
            if (DeviceInfo.DeviceType == DeviceType.Virtual)
            {
                return DeviceInfo.Platform == DevicePlatform.iOS
                    ? new[] { AppConstants.ApiBaseUrlIosSimulator }
                    : new[] { AppConstants.ApiBaseUrlEmulator };
            }

            return AppConstants.ApiBaseUrlPhysicalCandidates;
        }

        public static async Task<string> ResolveAsync()
        {
            if (!string.IsNullOrEmpty(_cachedUrl))
                return _cachedUrl;

            foreach (var candidate in GetCandidates())
            {
                var normalized = candidate.TrimEnd('/');
                if (await IsReachableAsync(normalized))
                {
                    _cachedUrl = normalized;
                    return _cachedUrl;
                }
            }

            return GetCandidates()[0].TrimEnd('/');
        }

        public static void Remember(string baseUrl)
        {
            if (!string.IsNullOrWhiteSpace(baseUrl))
                _cachedUrl = baseUrl.TrimEnd('/');
        }

        private static async Task<bool> IsReachableAsync(string baseUrl)
        {
            try
            {
                using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) })
                {
                    var response = await client.GetAsync(
                        baseUrl + "/api/patients",
                        HttpCompletionOption.ResponseHeadersRead);

                    return response.IsSuccessStatusCode
                        || response.StatusCode == HttpStatusCode.Unauthorized;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
