using Medical_atention.Constants;
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
            // TODO: Remove before production — clears saved session to force login screen on every launch
            SecureStorage.RemoveAll();
            MainPage = new LoginPage();
            _ = CheckExistingSessionAsync();
        }

        private async Task CheckExistingSessionAsync()
        {
            try
            {
                var token = await SecureStorage.GetAsync(AppConstants.TokenKey);
                if (!string.IsNullOrEmpty(token))
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        MainPage = new AppShell();
                    });
                }
            }
            catch (Exception)
            {
                // SecureStorage not available on this platform; stay on LoginPage
            }
        }

        protected override void OnStart() { }
        protected override void OnSleep() { }
        protected override void OnResume() { }
    }
}
