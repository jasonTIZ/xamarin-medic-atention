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

        public PatientListViewModel() : this(new PatientService()) { }

        public PatientListViewModel(IPatientService patientService)
        {
            _patientService = patientService;
            RefreshCommand = new Command(async () => await RefreshAsync());
            PatientSelectedCommand = new Command<Patient>(async p => await OnPatientSelectedAsync(p));
            Connectivity.ConnectivityChanged += OnConnectivityChanged;
            MessagingCenter.Subscribe<object>(
                this, LocalDataChangedHelper.PatientsChangedMessage, _ => ReloadFromLocalOnMainThread());
            MessagingCenter.Subscribe<object, int>(
                this, SyncNotificationHelper.SyncCompletedMessage, (_, __) => ReloadFromLocalOnMainThread());
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

        public async Task OnAppearingAsync()
        {
            UpdateOfflineState();
            await LoadAsync(forceRefresh: _patientService.IsOnline());
        }

        public async Task RefreshAsync()
        {
            IsRefreshing = true;
            try
            {
                await LoadAsync(forceRefresh: _patientService.IsOnline());
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
                IReadOnlyList<Patient> patients;
                if (!_patientService.IsOnline())
                    patients = await _patientService.LoadPatientsFromLocalAsync();
                else
                    patients = await _patientService.LoadPatientsAsync(forceRefresh);

                _allPatients = patients.ToList();
                ApplyFilter();
            }
            catch (Exception)
            {
                _allPatients = (await _patientService.LoadPatientsFromLocalAsync()).ToList();
                ApplyFilter();
            }
            finally
            {
                IsLoading = false;
                UpdateOfflineState();
            }
        }

        private void ReloadFromLocalOnMainThread()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                if (IsLoading || IsRefreshing) return;
                _allPatients = (await _patientService.LoadPatientsFromLocalAsync()).ToList();
                ApplyFilter();
            });
        }

        private void ApplyFilter()
        {
            var query = SanitizeSearchInput(_searchText);
            var filtered = string.IsNullOrEmpty(query)
                ? _allPatients
                : FilterByQuery(_allPatients, query);

            ReplaceFilteredList(filtered);
        }

        private static List<Patient> FilterByQuery(List<Patient> source, string query)
        {
            var queryLower = query.ToLowerInvariant();
            var queryNormalized = NormalizeForComparison(query);

            return source.Where(p =>
            {
                var fullName = p.FullName ?? string.Empty;
                var nameMatch = fullName.ToLowerInvariant().Contains(queryLower)
                    || NormalizeForComparison(fullName).Contains(queryNormalized);

                var documentNormalized = NormalizeForComparison(p.DocumentNumber);
                var documentMatch = !string.IsNullOrEmpty(queryNormalized)
                    && documentNormalized.Contains(queryNormalized);

                return nameMatch || documentMatch;
            }).ToList();
        }

        /// <summary>
        /// Reemplaza la colección visible para que badges y prioridad se actualicen en la UI.
        /// </summary>
        private void ReplaceFilteredList(List<Patient> filtered)
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
            await Shell.Current.GoToAsync($"PatientDetailPage?id={patient.Id}");
        }

        private void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                UpdateOfflineState();
                if (e.NetworkAccess == NetworkAccess.Internet)
                    await LoadAsync(forceRefresh: true);
                else
                    ReloadFromLocalOnMainThread();
            });
        }

        private void UpdateOfflineState()
            => IsOffline = !_patientService.IsOnline();

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
