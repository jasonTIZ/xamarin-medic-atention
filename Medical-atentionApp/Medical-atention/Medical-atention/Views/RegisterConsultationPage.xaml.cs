using Medical_atention.ViewModels;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Medical_atention.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [QueryProperty(nameof(PatientId), "patientId")]
    [QueryProperty(nameof(PatientName), "patientName")]
    [QueryProperty(nameof(PatientCedula), "patientCedula")]
    public partial class RegisterConsultationPage : ContentPage
    {
        private readonly RegisterConsultationViewModel _viewModel = new RegisterConsultationViewModel();
        private int _patientId;
        private string _patientName = string.Empty;
        private string _patientCedula = string.Empty;

        public RegisterConsultationPage()
        {
            InitializeComponent();
            BindingContext = _viewModel;
        }

        public string PatientId
        {
            set
            {
                if (int.TryParse(value, out var id))
                {
                    _patientId = id;
                    TryInitialize();
                }
            }
        }

        public string PatientName
        {
            set
            {
                _patientName = string.IsNullOrEmpty(value) ? string.Empty : Uri.UnescapeDataString(value);
                TryInitialize();
            }
        }

        public string PatientCedula
        {
            set
            {
                _patientCedula = string.IsNullOrEmpty(value) ? string.Empty : Uri.UnescapeDataString(value);
                TryInitialize();
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            OfflineBanner?.Refresh();
        }

        private void TryInitialize()
        {
            if (_patientId > 0)
                _viewModel.Initialize(_patientId, _patientName, _patientCedula);
        }
    }
}
