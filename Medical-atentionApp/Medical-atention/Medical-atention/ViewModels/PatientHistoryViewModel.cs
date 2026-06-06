using Medical_atention.Constants;
using Medical_atention.Models;
using Medical_atention.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Medical_atention.ViewModels
{
    public class PatientHistoryViewModel : INotifyPropertyChanged
    {
        private readonly IPatientService _patientService;
        private int _patientId;
        private string _patientName = string.Empty;
        private int _totalConsultations;
        private string _lastConsultationDate = string.Empty;
        private string _currentPriority = string.Empty;
        private List<DiagnosisSummary> _recentDiagnoses = new List<DiagnosisSummary>();
        private List<string> _frequentMedications = new List<string>();
        private List<PriorityPoint> _priorityEvolution = new List<PriorityPoint>();
        private bool _isLoading;
        private bool _isError;

        public PatientHistoryViewModel() : this(new PatientService()) { }

        public PatientHistoryViewModel(IPatientService patientService)
        {
            _patientService = patientService;
        }

        public int PatientId => _patientId;
        public string PatientName => _patientName;

        public int TotalConsultations
        {
            get => _totalConsultations;
            set { _totalConsultations = value; OnPropertyChanged(); }
        }

        public string LastConsultationDate
        {
            get => _lastConsultationDate;
            set { _lastConsultationDate = value; OnPropertyChanged(); }
        }

        public string CurrentPriority
        {
            get => _currentPriority;
            set { _currentPriority = value; OnPropertyChanged(); }
        }

        public List<DiagnosisSummary> RecentDiagnoses
        {
            get => _recentDiagnoses;
            set { _recentDiagnoses = value; OnPropertyChanged(); }
        }

        public List<string> FrequentMedications
        {
            get => _frequentMedications;
            set { _frequentMedications = value; OnPropertyChanged(); }
        }

        public List<PriorityPoint> PriorityEvolution
        {
            get => _priorityEvolution;
            set { _priorityEvolution = value; OnPropertyChanged(); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public bool IsError
        {
            get => _isError;
            set { _isError = value; OnPropertyChanged(); }
        }

        public bool HasPriority => !string.IsNullOrWhiteSpace(_currentPriority);

        public void Initialize(int patientId, string patientName)
        {
            _patientId = patientId;
            _patientName = patientName;
            _ = LoadAsync();
        }

        public async Task LoadAsync()
        {
            if (_patientId <= 0) return;

            IsLoading = true;
            IsError = false;
            try
            {
                var token = await SecureStorage.GetAsync(AppConstants.TokenKey);
                var summary = await _patientService.GetPatientHistoryAsync(_patientId, token);

                if (summary == null)
                {
                    IsError = true;
                    return;
                }

                TotalConsultations = summary.TotalConsultations;
                LastConsultationDate = summary.LastConsultationDate ?? string.Empty;
                CurrentPriority = summary.CurrentPriority ?? string.Empty;
                RecentDiagnoses = summary.RecentDiagnoses;
                FrequentMedications = summary.FrequentMedications;
                PriorityEvolution = summary.PriorityEvolution;
            }
            catch (Exception)
            {
                IsError = true;
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
