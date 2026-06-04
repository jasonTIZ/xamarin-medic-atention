using Medical_atention.Models;
using Medical_atention.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Medical_atention.ViewModels
{
    public class PatientsViewModel : INotifyPropertyChanged
    {
        private readonly IPatientService _patientService;
        private ObservableCollection<Patient> _patients = new ObservableCollection<Patient>();
        private bool _isLoading;
        private bool _isEmpty;

        public PatientsViewModel() : this(new PatientService()) { }

        public PatientsViewModel(IPatientService patientService)
        {
            _patientService = patientService;
        }

        public ObservableCollection<Patient> Patients
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
                var list = await _patientService.LoadPatientsAsync();
                Patients = new ObservableCollection<Patient>(list);
                IsEmpty = !Patients.Any();
            }
            catch (Exception) { }
            finally { IsLoading = false; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
