using System.Collections.Generic;
using Android.App;
using Android.Content;
using Firebase.Messaging;
using Medical_atention.Services;

namespace Medical_atention.Droid.Services
{
    // Recibe el token FCM y los mensajes push entrantes. Se registra vía atributos
    // (IntentFilter MESSAGING_EVENT), por lo que no requiere entradas manuales en el manifest.
    [Service(Exported = false)]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class AppFirebaseMessagingService : FirebaseMessagingService
    {
        public override void OnNewToken(string token)
        {
            base.OnNewToken(token);
            PushNotificationService.Instance.OnTokenReceived(token, "android");
        }

        public override void OnMessageReceived(RemoteMessage message)
        {
            base.OnMessageReceived(message);

            var data = message.Data ?? new Dictionary<string, string>();
            var notification = message.GetNotification();

            var title = notification?.Title;
            var body = notification?.Body;

            // Soporta también mensajes "data-only" con claves title/body.
            if (string.IsNullOrEmpty(title) && data.TryGetValue("title", out var dataTitle))
                title = dataTitle;
            if (string.IsNullOrEmpty(body) && data.TryGetValue("body", out var dataBody))
                body = dataBody;

            // OnMessageReceived para mensajes "notification" solo se invoca en primer plano.
            PushNotificationService.Instance.HandleForegroundMessage(title, body, data);
        }
    }
}
