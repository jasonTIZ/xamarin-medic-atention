using Medical_atention.Constants;
using Medical_atention.Data;
using Medical_atention.Helpers;
using Medical_atention.Models;
using Medical_atention.Services;
using System;
using System.Collections.Generic;
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
        private readonly ObservableCollection<PatientResponseDto> _filteredPatients =
            new ObservableCollection<PatientResponseDto>();
        private List<PatientResponseDto> _allPatients = new List<PatientResponseDto>();
        private string _searchText = string.Empty;
        private bool _isLoading;
        private bool _isEmpty;

        public PatientsViewModel() : this(new PatientService()) { }

        public PatientsViewModel(IPatientService patientService)
        {
            _patientService = patientService;
        }

        public ObservableCollection<PatientResponseDto> FilteredPatients => _filteredPatients;

        public string SearchText
        {
            get => _searchText;
            set
            {
                var sanitized = SanitizeSearchInput(value);
                if (_searchText == sanitized) return;
                _searchText = sanitized;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EmptyMessage));
                OnPropertyChanged(nameof(ShowSearchEmpty));
                ApplyFilter();
            }
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

        public string EmptyMessage =>
            string.IsNullOrWhiteSpace(_searchText)
                ? "Sin pacientes registrados"
                : "No se encontraron pacientes con ese criterio";

        public bool HasFilteredResults => _filteredPatients.Any();

        public bool ShowSearchEmpty =>
            !IsEmpty && !string.IsNullOrWhiteSpace(_searchText) && !HasFilteredResults;

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

                _allPatients = patients.ToList();
                ApplyFilter();
            }
            catch (Exception) { }
            finally { IsLoading = false; }
        }

        private void ApplyFilter()
        {
            var query = SanitizeSearchInput(_searchText);
            var filtered = string.IsNullOrEmpty(query)
                ? _allPatients
                : FilterByQuery(_allPatients, query);

            ReplaceFilteredList(filtered);
            IsEmpty = !_allPatients.Any();
            OnPropertyChanged(nameof(HasFilteredResults));
            OnPropertyChanged(nameof(ShowSearchEmpty));
        }

        private static List<PatientResponseDto> FilterByQuery(List<PatientResponseDto> source, string query)
        {
            var queryNormalized = StringNormalizationHelper.NormalizeForSearch(query);
            if (string.IsNullOrEmpty(queryNormalized))
                return source.ToList();

            return source.Where(p =>
                StringNormalizationHelper.ContainsNormalized(p.FullName, queryNormalized, queryAlreadyNormalized: true)
                || StringNormalizationHelper.ContainsNormalized(
                    p.IdentificationNumber, queryNormalized, queryAlreadyNormalized: true)
            ).ToList();
        }

        private void ReplaceFilteredList(List<PatientResponseDto> filtered)
        {
            _filteredPatients.Clear();
            foreach (var patient in filtered)
                _filteredPatients.Add(patient);
        }

        private static string SanitizeSearchInput(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var parts = value.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", parts);
        }

        private static void ApplyLatestPriority(
            PatientResponseDto patient,
            Dictionary<int, (string Priority, DateTime ConsultationDate)> localPriorities)
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
