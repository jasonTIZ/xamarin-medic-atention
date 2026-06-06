using Medical_atention.Constants;
using Medical_atention.Helpers;
using Medical_atention.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Medical_atention.Services
{
    // Gestiona el ciclo de vida de las notificaciones push (FCM/APNs) en la capa compartida:
    // registra el token en el backend y reenvía a la UI los eventos de primer plano / apertura.
    // Las plataformas (Android/iOS) llaman a Instance.OnTokenReceived / HandleForegroundMessage /
    // HandleNotificationOpened.
    public class PushNotificationService : IPushNotificationService
    {
        public const string ForegroundMessage = "PushForegroundMessage";
        public const string NavigateMessage = "PushNavigateMessage";

        private static readonly Lazy<PushNotificationService> _instance =
            new Lazy<PushNotificationService>(() => new PushNotificationService());

        public static PushNotificationService Instance => _instance.Value;

        private string _token;
        private string _platform;

        // Navegación pendiente cuando la notificación se abre antes de que la sesión esté lista.
        public PushMessage PendingNavigation { get; private set; }

        public void OnTokenReceived(string token, string platform)
        {
            if (string.IsNullOrWhiteSpace(token)) return;
            _token = token;
            _platform = platform;
            _ = TryRegisterAsync(token, platform);
        }

        public Task<bool> RegisterPendingTokenAsync()
        {
            if (string.IsNullOrEmpty(_token)) return Task.FromResult(false);
            return TryRegisterAsync(_token, _platform);
        }

        private async Task<bool> TryRegisterAsync(string token, string platform)
        {
            try
            {
                var userIdStr = await SecureStorage.GetAsync(AppConstants.UserIdKey);
                if (string.IsNullOrEmpty(userIdStr))
                    return false; // aún sin sesión; se reintenta tras el login

                int.TryParse(userIdStr, out var userId);

                var payload = new DeviceRegisterRequest
                {
                    Token = token,
                    Platform = platform,
                    UserId = userId
                };

                using (var client = await ApiClient.CreateAsync())
                {
                    var content = new StringContent(
                        JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync("api/devices/register", content);
                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PushNotificationService] Error registrando token: {ex.Message}");
                return false;
            }
        }

        public void HandleForegroundMessage(string title, string body, IDictionary<string, string> data)
        {
            var message = new PushMessage
            {
                Title = string.IsNullOrWhiteSpace(title) ? "Notificación" : title,
                Body = body ?? string.Empty,
                Data = Normalize(data)
            };
            MessagingCenter.Send<object, PushMessage>(this, ForegroundMessage, message);
        }

        public void HandleNotificationOpened(IDictionary<string, string> data)
        {
            var message = new PushMessage { Data = Normalize(data) };
            PendingNavigation = message;
            MessagingCenter.Send<object, PushMessage>(this, NavigateMessage, message);
        }

        public void ClearPendingNavigation() => PendingNavigation = null;

        // Filtra las claves internas de FCM y deja solo el payload de datos de la app.
        private static Dictionary<string, string> Normalize(IDictionary<string, string> data)
        {
            var dict = new Dictionary<string, string>();
            if (data == null) return dict;

            foreach (var kv in data)
            {
                if (string.IsNullOrEmpty(kv.Key)) continue;
                if (kv.Key.StartsWith("google.", StringComparison.OrdinalIgnoreCase)) continue;
                if (kv.Key == "from" || kv.Key == "collapse_key" || kv.Key == "message_type") continue;
                dict[kv.Key] = kv.Value;
            }
            return dict;
        }
    }
}
