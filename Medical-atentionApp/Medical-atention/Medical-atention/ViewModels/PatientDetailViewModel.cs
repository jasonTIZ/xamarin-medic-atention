using Medical_atention.Constants;
using Medical_atention.Models;
using Medical_atention.Services;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Medical_atention.ViewModels
{
    public class PatientDetailViewModel : INotifyPropertyChanged
    {
        private readonly IPatientService _patientService;
        private PatientResponseDto _patient;
        private bool _isLoading;

        public PatientDetailViewModel(int id) : this(id, new PatientService()) { }

        public PatientDetailViewModel(int id, IPatientService patientService)
        {
            _patientService = patientService;
            _ = LoadPatientAsync(id);
        }

        public PatientResponseDto Patient
        {
            get => _patient;
            set { _patient = value; OnPropertyChanged(); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNotLoading)); }
        }

        public bool IsNotLoading => !_isLoading;

        private async Task LoadPatientAsync(int id)
        {
            IsLoading = true;
            try
            {
                var patient = await _patientService.GetPatientAsync(id, await SecureStorage.GetAsync(AppConstants.TokenKey));
                Device.BeginInvokeOnMainThread(() => Patient = patient);
            }
            catch (Exception) { }
            finally
            {
                Device.BeginInvokeOnMainThread(() => IsLoading = false);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
