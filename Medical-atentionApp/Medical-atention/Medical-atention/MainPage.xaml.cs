using Medical_atention.Constants;
using Medical_atention.Models;
using Medical_atention.Services;
using Medical_atention.ViewModels;
using Medical_atention.Views;
using System;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Medical_atention
{
    public partial class MainPage : ContentPage
    {
        private const double MenuWidth = 260;
        private const double MenuHeight = 220;

        private readonly PatientsViewModel _viewModel;
        private readonly IPatientService _patientService = new PatientService();
        private PatientResponseDto _menuPatient;

        public MainPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new PatientsViewModel();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadPatientsAsync();
        }

        private void OnMenuClicked(object sender, EventArgs e)
        {
            if (!(((Button)sender).CommandParameter is PatientResponseDto patient))
                return;

            _menuPatient = patient;
            MenuOverlay.IsVisible = true;
            MenuPopup.IsVisible = true;

            if (sender is View anchor)
                Device.BeginInvokeOnMainThread(() => PositionMenuNear(anchor));
        }

        private void PositionMenuNear(View anchor)
        {
            var (x, y) = GetPositionOnPage(anchor);
            var menuX = Math.Max(8, x + anchor.Width - MenuWidth);
            var menuY = y + anchor.Height + 4;

            var maxY = RootLayout.Height - MenuHeight - 8;
            if (maxY > 0 && menuY > maxY)
                menuY = Math.Max(8, y - MenuHeight - 4);

            AbsoluteLayout.SetLayoutBounds(MenuPopup, new Rectangle(menuX, menuY, MenuWidth, MenuHeight));
            AbsoluteLayout.SetLayoutFlags(MenuPopup, AbsoluteLayoutFlags.None);
        }

        private (double x, double y) GetPositionOnPage(VisualElement element)
        {
            double x = element.X;
            double y = element.Y;
            var parent = element.Parent as VisualElement;

            while (parent != null)
            {
                x += parent.X;
                y += parent.Y;
                if (parent == RootLayout)
                    break;
                parent = parent.Parent as VisualElement;
            }

            return (x, y);
        }

        private void OnMenuDismissed(object sender, EventArgs e) => CloseMenu();

        private void CloseMenu()
        {
            MenuOverlay.IsVisible = false;
            MenuPopup.IsVisible = false;
            _menuPatient = null;
        }

        private async void OnMenuDetail(object sender, EventArgs e)
        {
            var patient = _menuPatient;
            CloseMenu();
            if (patient != null)
                await Shell.Current.GoToAsync($"{nameof(PatientDetailPage)}?id={patient.Id}");
        }

        private async void OnMenuHistorial(object sender, EventArgs e)
        {
            CloseMenu();
            await DisplayAlert("Historial", "Función no disponible aún.", "OK");
        }

        private async void OnMenuNewConsultation(object sender, EventArgs e)
        {
            CloseMenu();
            await DisplayAlert("Nueva consulta", "Función no disponible aún.", "OK");
        }

        private async void OnMenuDelete(object sender, EventArgs e)
        {
            var patient = _menuPatient;
            CloseMenu();
            if (patient is null) return;

            bool confirm = await DisplayAlert(
                "Eliminar paciente",
                $"¿Eliminar a {patient.FullName}?",
                "Eliminar", "Cancelar");

            if (!confirm) return;

            try
            {
                var token = await SecureStorage.GetAsync(AppConstants.TokenKey);
                if (!await _patientService.DeleteAsync(patient.Id, token))
                {
                    await DisplayAlert("Error", "No se pudo eliminar el paciente.", "OK");
                    return;
                }
                await _viewModel.LoadPatientsAsync();
            }
            catch (Exception)
            {
                await DisplayAlert("Error", "Sin conexión, verifica tu red.", "OK");
            }
        }

        private async void OnAddPatientTapped(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(RegisterPatientPage));
        }
    }
}
