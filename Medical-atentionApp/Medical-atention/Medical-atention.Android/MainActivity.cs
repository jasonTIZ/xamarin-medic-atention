using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Tasks;
using Android.Runtime;
using Android.OS;
using Firebase.Messaging;
using Medical_atention.Services;
using Plugin.LocalNotification;

namespace Medical_atention.Droid
{
    [Activity(Label = "Medical_atention", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private const int PostNotificationsRequestCode = 1001;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Crea el canal de notificaciones (requerido en API 26+) antes de cargar la app.
            NotificationCenter.CreateNotificationChannel();
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
            // Procesa el toque sobre una notificación local que abrió la app.
            NotificationCenter.NotifyNotificationTapped(Intent);
            RequestNotificationPermissionIfNeeded();

            // Firebase Cloud Messaging.
            Firebase.FirebaseApp.InitializeApp(this);
            FetchFcmToken();
            HandlePushIntent(Intent);
        }

        protected override void OnNewIntent(Intent intent)
        {
            NotificationCenter.NotifyNotificationTapped(intent);
            HandlePushIntent(intent);
            base.OnNewIntent(intent);
        }

        // Obtiene el token FCM actual y lo entrega a la capa compartida para registrarlo.
        private void FetchFcmToken()
        {
            try
            {
                FirebaseMessaging.Instance.GetToken().AddOnCompleteListener(new FcmTokenListener());
            }
            catch (Exception)
            {
                // Si Firebase no está configurado en este dispositivo, se omite.
            }
        }

        // Extrae el payload de datos de un intent originado por el toque de una notificación push.
        private void HandlePushIntent(Intent intent)
        {
            var extras = intent?.Extras;
            if (extras == null) return;

            // Marcadores que indican que el intent proviene de FCM.
            var looksLikePush = extras.ContainsKey("from") || extras.ContainsKey("google.message_id");
            if (!looksLikePush) return;

            var data = new Dictionary<string, string>();
            foreach (var key in extras.KeySet())
            {
                var value = extras.GetString(key);
                if (value != null) data[key] = value;
            }

            PushNotificationService.Instance.HandleNotificationOpened(data);
        }

        // En Android 13 (API 33) las notificaciones requieren permiso en tiempo de ejecución.
        // Se usa el entero 33 en lugar de BuildVersionCodes.Tiramisu para compilar también
        // contra bindings de Android 12.1 (API 32), donde ese valor del enum no existe.
        private void RequestNotificationPermissionIfNeeded()
        {
            if ((int)Build.VERSION.SdkInt < 33)
                return;

            const string permission = "android.permission.POST_NOTIFICATIONS";
            if (CheckSelfPermission(permission) != Permission.Granted)
                RequestPermissions(new[] { permission }, PostNotificationsRequestCode);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            // Manejo del rechazo de permiso de notificaciones: si el usuario lo niega,
            // la app sigue funcionando y el token igual se registra (solo no se mostrarán
            // notificaciones del sistema hasta que el permiso se conceda en Ajustes).
            if (requestCode == PostNotificationsRequestCode
                && (grantResults.Length == 0 || grantResults[0] != Permission.Granted))
            {
                System.Diagnostics.Debug.WriteLine(
                    "[MainActivity] Permiso POST_NOTIFICATIONS denegado por el usuario.");
            }

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnActivityResult(int requestCode, Android.App.Result resultCode, Android.Content.Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
        }
    }

    // Listener para el resultado asíncrono de FirebaseMessaging.GetToken().
    internal class FcmTokenListener : Java.Lang.Object, IOnCompleteListener
    {
        public void OnComplete(Android.Gms.Tasks.Task task)
        {
            if (task.IsSuccessful && task.Result != null)
            {
                var token = task.Result.ToString();
                PushNotificationService.Instance.OnTokenReceived(token, "android");
            }
        }
    }
}
