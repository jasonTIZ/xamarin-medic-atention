using Medical_atention.Models;
using Medical_atention.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Medical_atention.ViewModels
{
    public class PatientListViewModel : INotifyPropertyChanged
    {
        private readonly IPatientService _patientService;
        private readonly ObservableCollection<Patient> _filteredPatients = new ObservableCollection<Patient>();
        private List<Patient> _allPatients = new List<Patient>();
        private string _searchText = string.Empty;
        private bool _isLoading;
        private bool _isRefreshing;
        private bool _isOffline;
        private bool _hasLoaded;

        public PatientListViewModel() : this(new PatientService()) { }

        public PatientListViewModel(IPatientService patientService)
        {
            _patientService = patientService;
            RefreshCommand = new Command(async () => await RefreshAsync());
            PatientSelectedCommand = new Command<Patient>(async p => await OnPatientSelectedAsync(p));
            Connectivity.ConnectivityChanged += OnConnectivityChanged;
            UpdateOfflineState();
        }

        public ObservableCollection<Patient> FilteredPatients => _filteredPatients;

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
                ApplyFilter();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNotLoading)); }
        }

        public bool IsNotLoading => !IsLoading;

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set { _isRefreshing = value; OnPropertyChanged(); }
        }

        public bool IsOffline
        {
            get => _isOffline;
            set { _isOffline = value; OnPropertyChanged(); }
        }

        public string EmptyMessage =>
            string.IsNullOrWhiteSpace(_searchText)
                ? "No hay pacientes registrados"
                : "No se encontraron pacientes con ese criterio";

        public ICommand RefreshCommand { get; }
        public ICommand PatientSelectedCommand { get; }

        public async Task InitializeAsync()
        {
            if (_hasLoaded) return;
            await LoadAsync(forceRefresh: false);
            _hasLoaded = true;
        }

        public async Task RefreshAsync()
        {
            IsRefreshing = true;
            try
            {
                await LoadAsync(forceRefresh: true);
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private async Task LoadAsync(bool forceRefresh)
        {
            IsLoading = true;
            UpdateOfflineState();
            try
            {
                var patients = await _patientService.LoadPatientsAsync(forceRefresh);
                _allPatients = patients.ToList();
                ApplyFilter();
            }
            catch (Exception)
            {
                _allPatients = (await _patientService.LoadPatientsAsync(forceRefresh: false)).ToList();
                ApplyFilter();
            }
            finally
            {
                IsLoading = false;
                UpdateOfflineState();
            }
        }

        private void ApplyFilter()
        {
            var query = SanitizeSearchInput(_searchText);
            if (string.IsNullOrEmpty(query))
            {
                ReplaceFilteredList(_allPatients);
                return;
            }

            var queryLower = query.ToLowerInvariant();
            var queryNormalized = NormalizeForComparison(query);

            var filtered = _allPatients.Where(p =>
            {
                var fullName = p.FullName ?? string.Empty;
                var nameMatch = fullName.ToLowerInvariant().Contains(queryLower)
                    || NormalizeForComparison(fullName).Contains(queryNormalized);

                var documentNormalized = NormalizeForComparison(p.DocumentNumber);
                var documentMatch = !string.IsNullOrEmpty(queryNormalized)
                    && documentNormalized.Contains(queryNormalized);

                return nameMatch || documentMatch;
            }).ToList();

            ReplaceFilteredList(filtered);
        }

        private void ReplaceFilteredList(List<Patient> filtered)
        {

            for (var i = _filteredPatients.Count - 1; i >= 0; i--)
            {
                if (!filtered.Any(f => f.Id == _filteredPatients[i].Id))
                    _filteredPatients.RemoveAt(i);
            }

            foreach (var patient in filtered)
            {
                if (!_filteredPatients.Any(p => p.Id == patient.Id))
                    _filteredPatients.Add(patient);
            }

            for (var i = 0; i < filtered.Count; i++)
            {
                var item = _filteredPatients.FirstOrDefault(p => p.Id == filtered[i].Id);
                if (item == null) continue;
                var currentIndex = _filteredPatients.IndexOf(item);
                if (currentIndex >= 0 && currentIndex != i)
                    _filteredPatients.Move(currentIndex, i);
            }
        }

        /// <summary>
        /// Trim and collapse extra spaces in the search box text.
        /// </summary>
        private static string SanitizeSearchInput(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var parts = value.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", parts);
        }

        /// <summary>
        /// Remove spaces, dashes and symbols so cédula "1-2345-6789" matches "123456789" or "2345".
        /// </summary>
        private static string NormalizeForComparison(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var chars = value.Where(char.IsLetterOrDigit).ToArray();
            return new string(chars).ToLowerInvariant();
        }

        private async Task OnPatientSelectedAsync(Patient patient)
        {
            if (patient == null) return;
            await Shell.Current.GoToAsync($"patientdetail?patientId={patient.Id}");
        }

        private void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
            => Device.BeginInvokeOnMainThread(UpdateOfflineState);

        private void UpdateOfflineState()
            => IsOffline = !_patientService.IsOnline();

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
