using Medical_atention.Models;
using Medical_atention.ViewModels;
using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Medical_atention.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [QueryProperty(nameof(PatientId), "patientId")]
    [QueryProperty(nameof(PatientName), "patientName")]
    [QueryProperty(nameof(PatientCedula), "patientCedula")]
    public partial class ConsultationHistoryPage : ContentPage
    {
        private readonly ConsultationHistoryViewModel _viewModel = new ConsultationHistoryViewModel();
        private int _patientId;
        private string _patientName = string.Empty;
        private string _patientCedula = string.Empty;

        public ConsultationHistoryPage()
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
            if (_patientId > 0)
                _ = _viewModel.LoadAsync();
        }

        private void TryInitialize()
        {
            if (_patientId > 0)
                _viewModel.Initialize(_patientId, _patientName, _patientCedula);
        }

        private async void OnConsultationSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is ConsultationListItem item)
            {
                ((CollectionView)sender).SelectedItem = null;
                await Shell.Current.GoToAsync(
                    $"{nameof(ConsultationDetailPage)}?consultationId={item.ServerId ?? 0}&localId={item.LocalId}&patientId={_patientId}");
            }
        }
    }
}
