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
