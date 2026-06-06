using System.Collections.Generic;
using System.Threading.Tasks;

namespace Medical_atention.Services
{
    public interface IPushNotificationService
    {
        // La plataforma entrega el token FCM/APNs; el servicio intenta registrarlo en el backend.
        void OnTokenReceived(string token, string platform);

        // Reintenta el registro del último token (p. ej. tras iniciar sesión).
        Task<bool> RegisterPendingTokenAsync();

        // Notificación recibida con la app en primer plano.
        void HandleForegroundMessage(string title, string body, IDictionary<string, string> data);

        // Notificación tocada/abierta desde segundo plano.
        void HandleNotificationOpened(IDictionary<string, string> data);
    }
}
