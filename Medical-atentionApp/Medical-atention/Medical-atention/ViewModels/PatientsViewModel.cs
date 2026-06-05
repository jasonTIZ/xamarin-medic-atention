using Medical_atention.Constants;
using Medical_atention.Data;
using Medical_atention.Models;
using Medical_atention.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Medical_atention.ViewModels
{
    public class PatientsViewModel : INotifyPropertyChanged
    {
        private readonly IPatientService _patientService;
        private ObservableCollection<PatientResponseDto> _patients = new ObservableCollection<PatientResponseDto>();
        private bool _isLoading;
        private bool _isEmpty;

        public PatientsViewModel() : this(new PatientService()) { }

        public PatientsViewModel(IPatientService patientService)
        {
            _patientService = patientService;
        }

        public ObservableCollection<PatientResponseDto> Patients
        {
            get => _patients;
            set { _patients = value; OnPropertyChanged(); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public bool IsEmpty
        {
            get => _isEmpty;
            set { _isEmpty = value; OnPropertyChanged(); }
        }

        public async Task LoadPatientsAsync()
        {
            IsLoading = true;
            try
            {
                var patients = await _patientService.GetAllPatientsAsync(
                    await SecureStorage.GetAsync(AppConstants.TokenKey));

                var localPriorities = await LocalDatabase.Instance.GetLatestPrioritiesByPatientAsync();
                foreach (var patient in patients)
                    ApplyLatestPriority(patient, localPriorities);

                Patients = new ObservableCollection<PatientResponseDto>(patients);
                IsEmpty = !Patients.Any();
            }
            catch (Exception) { }
            finally { IsLoading = false; }
        }

        private static void ApplyLatestPriority(
            PatientResponseDto patient,
            System.Collections.Generic.Dictionary<int, (string Priority, DateTime ConsultationDate)> localPriorities)
        {
            if (localPriorities.TryGetValue(patient.Id, out var local))
            {
                if (patient.LastConsultationDate == null || local.ConsultationDate > patient.LastConsultationDate)
                {
                    patient.Priority = local.Priority;
                    patient.LastConsultationDate = local.ConsultationDate;
                }
            }

            if (string.IsNullOrWhiteSpace(patient.Priority))
                patient.Priority = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
