using Medical_atention.Constants;
using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Medical_atention.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(RegisterPatientPage), typeof(RegisterPatientPage));
            Routing.RegisterRoute(nameof(PatientDetailPage), typeof(PatientDetailPage));
            Routing.RegisterRoute(nameof(ConsultationDetailPage), typeof(ConsultationDetailPage));
            Routing.RegisterRoute(nameof(RegisterConsultationPage), typeof(RegisterConsultationPage));
            Routing.RegisterRoute(nameof(ConsultationHistoryPage), typeof(ConsultationHistoryPage));
            Routing.RegisterRoute(nameof(ImageViewerPage), typeof(ImageViewerPage));
            Routing.RegisterRoute(nameof(PatientHistoryPage), typeof(PatientHistoryPage));
            _ = LoadUserInfoAsync();
        }

        private async System.Threading.Tasks.Task LoadUserInfoAsync()
        {
            try
            {
                var name = await SecureStorage.GetAsync(AppConstants.UserNameKey);
                var role = await SecureStorage.GetAsync(AppConstants.UserRoleKey);
                FlyoutUserName.Text = name ?? "Usuario";
                FlyoutUserRole.Text = role ?? string.Empty;
            }
            catch (Exception) { }
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Cerrar sesión", "¿Deseas cerrar sesión?", "Sí", "Cancelar");
            if (!confirm) return;

            SecureStorage.RemoveAll();

            Device.BeginInvokeOnMainThread(() =>
            {
                Application.Current.MainPage = new LoginPage();
            });
        }
    }
}
