using Medical_atention.ViewModels;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Medical_atention.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [QueryProperty(nameof(PatientId), "patientId")]
    [QueryProperty(nameof(PatientName), "patientName")]
    public partial class PatientHistoryPage : ContentPage
    {
        private readonly PatientHistoryViewModel _viewModel = new PatientHistoryViewModel();
        private int _patientId;
        private string _patientName = string.Empty;

        public PatientHistoryPage()
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

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (_patientId > 0)
                _ = _viewModel.LoadAsync();
        }

        private void TryInitialize()
        {
            if (_patientId > 0)
                _viewModel.Initialize(_patientId, _patientName);
        }

        private async void OnViewFullHistoryClicked(object sender, EventArgs e)
        {
            var name = Uri.EscapeDataString(_viewModel.PatientName);
            await Shell.Current.GoToAsync(
                $"{nameof(ConsultationHistoryPage)}?patientId={_viewModel.PatientId}&patientName={name}&patientCedula=");
        }
    }
}
