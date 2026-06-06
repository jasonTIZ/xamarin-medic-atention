using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
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
            // Procesa el toque sobre una notificación que abrió la app.
            NotificationCenter.NotifyNotificationTapped(Intent);
            RequestNotificationPermissionIfNeeded();
        }

        protected override void OnNewIntent(Intent intent)
        {
            NotificationCenter.NotifyNotificationTapped(intent);
            base.OnNewIntent(intent);
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

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}