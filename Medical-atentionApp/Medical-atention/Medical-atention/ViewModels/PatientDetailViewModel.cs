using Medical_atention.Models;
using Medical_atention.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Medical_atention.ViewModels
{
    public class PatientDetailViewModel : INotifyPropertyChanged
    {
        private readonly IPatientService _patientService;
        private Patient _patient;
        private bool _isLoading = true;

        public PatientDetailViewModel() : this(new PatientService()) { }

        public PatientDetailViewModel(IPatientService patientService)
        {
            _patientService = patientService;
        }

        public Patient Patient
        {
            get => _patient;
            set { _patient = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasPatient)); }
        }

        public bool HasPatient => Patient != null;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public async Task LoadAsync(int patientId)
        {
            IsLoading = true;
            try
            {
                Patient = await _patientService.GetPatientAsync(patientId);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
