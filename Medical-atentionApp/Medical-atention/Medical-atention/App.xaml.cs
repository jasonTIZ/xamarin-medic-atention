using Medical_atention.Constants;
using Medical_atention.Data;
using Medical_atention.Helpers;
using Medical_atention.Models;
using Medical_atention.Services;
using Medical_atention.Views;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using static Medical_atention.Helpers.ApiExceptionHandler;

namespace Medical_atention
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new LoginPage();
            MessagingCenter.Subscribe<object, int>(
                this, SyncNotificationHelper.SyncCompletedMessage, OnSyncCompleted);

            MessagingCenter.Subscribe<object>(
                this, UnauthorizedMessage, OnUnauthorized);

            // Notificaciones push: primer plano (alerta) y apertura (navegación).
            MessagingCenter.Subscribe<object, PushMessage>(
                this, PushNotificationService.ForegroundMessage, OnPushForeground);
            MessagingCenter.Subscribe<object, PushMessage>(
                this, PushNotificationService.NavigateMessage, OnPushNavigate);

            _ = InitializeAppAsync();
            _ = CheckExistingSessionAsync();
        }

        private async Task InitializeAppAsync()
        {
            try
            {
                await LocalDatabase.InitializeAsync();
                ConnectivityService.Instance.Start();
            }
            catch (Exception)
            {
                // La BD se reintentará en el primer acceso al repositorio.
            }
        }

        private async Task CheckExistingSessionAsync()
        {
            try
            {
                var token = await SecureStorage.GetAsync(AppConstants.TokenKey);
                if (!string.IsNullOrEmpty(token) && JwtHelper.IsTokenValid(token))
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        MainPage = new AppShell();
                    });
                    // Sesión activa: registra el token push y aplica navegación pendiente.
                    _ = PushNotificationService.Instance.RegisterPendingTokenAsync();
                    ApplyPendingPushNavigation();
                }
                else if (!string.IsNullOrEmpty(token))
                {
                    SecureStorage.RemoveAll();
                }
            }
            catch (Exception)
            {
                // SecureStorage not available on this platform; stay on LoginPage
            }
        }

        // Notificación recibida en primer plano → alerta.
        private void OnPushForeground(object sender, PushMessage message)
        {
            if (message == null) return;
            Device.BeginInvokeOnMainThread(async () =>
            {
                if (MainPage != null)
                    await MainPage.DisplayAlert(
                        message.Title ?? "Notificación",
                        message.Body ?? string.Empty,
                        "OK");
            });
        }

        // Notificación abierta desde segundo plano → navegación.
        private void OnPushNavigate(object sender, PushMessage message)
        {
            Device.BeginInvokeOnMainThread(async () => await NavigateFromPushAsync(message));
        }

        private void ApplyPendingPushNavigation()
        {
            var pending = PushNotificationService.Instance.PendingNavigation;
            if (pending != null)
                Device.BeginInvokeOnMainThread(async () => await NavigateFromPushAsync(pending));
        }

        private async Task NavigateFromPushAsync(PushMessage message)
        {
            if (message?.Data == null) return;

            // Si aún no hay sesión (Shell), se conserva para aplicarla tras el login.
            if (!(MainPage is AppShell)) return;

            try
            {
                if (message.Data.TryGetValue("patientId", out var patientId)
                    && !string.IsNullOrEmpty(patientId))
                {
                    await Shell.Current.GoToAsync($"patientdetail?id={patientId}");
                }

                PushNotificationService.Instance.ClearPendingNavigation();
            }
            catch (Exception)
            {
                // Navegación best-effort; no romper si la ruta no está disponible.
            }
        }

        private void OnUnauthorized(object sender)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                SecureStorage.RemoveAll();
                MainPage = new LoginPage();
            });
        }

        private void OnSyncCompleted(object sender, int syncedCount)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                if (MainPage is AppShell)
                    await MainPage.DisplayAlert(
                        "Sincronización",
                        $"{syncedCount} registros sincronizados",
                        "OK");
            });
        }

        protected override void OnStart() { }

        protected override void OnSleep()
        {
            ConnectivityService.Instance.Stop();
        }

        protected override void OnResume()
        {
            ConnectivityService.Instance.Start();
            _ = new SyncService().SyncPendingAsync();
        }
    }
}
