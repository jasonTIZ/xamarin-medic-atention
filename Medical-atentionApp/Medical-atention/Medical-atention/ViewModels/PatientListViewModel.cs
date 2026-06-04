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
                if (_searchText == value) return;
                _searchText = value;
                OnPropertyChanged();
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
            var query = _searchText?.Trim().ToLowerInvariant() ?? string.Empty;
            var filtered = string.IsNullOrEmpty(query)
                ? _allPatients
                : _allPatients.Where(p =>
                    (p.FullName?.ToLowerInvariant().Contains(query) ?? false) ||
                    (p.DocumentNumber?.ToLowerInvariant().Contains(query) ?? false)).ToList();

            _filteredPatients.Clear();
            foreach (var patient in filtered)
                _filteredPatients.Add(patient);
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
