using Medical_atention.Constants;
using Medical_atention.Data;
using Medical_atention.Helpers;
using Medical_atention.Services;
using Medical_atention.Views;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

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
