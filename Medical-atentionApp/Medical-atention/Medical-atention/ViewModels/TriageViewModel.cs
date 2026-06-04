using Medical_atention.Constants;
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
using Xamarin.Forms;

namespace Medical_atention.ViewModels
{
    public class TriageViewModel : INotifyPropertyChanged
    {
        private readonly IPatientService _patientService;
        private bool _isLoading;
        private bool _isOffline;
        private bool _isEmpty;
        private string _lastSyncText = string.Empty;
        private string _snackbarMessage = string.Empty;
        private bool _isSnackbarVisible;
        private bool _autoRefreshEnabled;

        public TriageViewModel() : this(new PatientService()) { }

        public TriageViewModel(IPatientService patientService)
        {
            _patientService = patientService;
            Groups = new ObservableCollection<TriagePriorityGroup>();
            foreach (var level in PriorityHelper.AllLevels)
                Groups.Add(new TriagePriorityGroup(level));
        }

        public ObservableCollection<TriagePriorityGroup> Groups { get; }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public bool IsOffline
        {
            get => _isOffline;
            set { _isOffline = value; OnPropertyChanged(); }
        }

        public bool IsEmpty
        {
            get => _isEmpty;
            set
            {
                _isEmpty = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasPatients));
            }
        }

        public bool HasPatients => !_isEmpty;

        public string LastSyncText
        {
            get => _lastSyncText;
            set { _lastSyncText = value; OnPropertyChanged(); }
        }

        public string SnackbarMessage
        {
            get => _snackbarMessage;
            set { _snackbarMessage = value; OnPropertyChanged(); }
        }

        public bool IsSnackbarVisible
        {
            get => _isSnackbarVisible;
            set { _isSnackbarVisible = value; OnPropertyChanged(); }
        }

        public void StartAutoRefresh()
        {
            _autoRefreshEnabled = true;
        }

        public void StopAutoRefresh()
        {
            _autoRefreshEnabled = false;
        }

        public bool ShouldAutoRefresh()
        {
            return _autoRefreshEnabled
                && Connectivity.NetworkAccess == NetworkAccess.Internet
                && !IsLoading;
        }

        public async Task LoadAsync(bool silent = false)
        {
            if (!silent) IsLoading = true;
            try
            {
                var token = await SecureStorage.GetAsync(AppConstants.TokenKey);
                await _patientService.SyncPendingPriorityChangesAsync(token);

                var (patients, fromCache, error) = await _patientService.GetPatientsByPriorityAsync(token);

                IsOffline = fromCache || Connectivity.NetworkAccess != NetworkAccess.Internet;
                await UpdateLastSyncTextAsync(fromCache);

                if (!string.IsNullOrEmpty(error) && patients.Count == 0)
                {
                    ShowSnackbar(error);
                    IsEmpty = true;
                    ClearGroups();
                    return;
                }

                RebuildGroups(patients);
                IsEmpty = !patients.Any();
            }
            catch (Exception)
            {
                ShowSnackbar("Error al cargar triaje");
            }
            finally
            {
                if (!silent) IsLoading = false;
            }
        }

        public async Task<bool> ChangePriorityAsync(TriagePatientItem item, PriorityLevel newPriority)
        {
            if (item == null || item.Priority == newPriority) return true;

            var previous = item.Priority;
            MoveItemToGroup(item, newPriority);

            var token = await SecureStorage.GetAsync(AppConstants.TokenKey);
            var (success, error) = await _patientService.UpdatePriorityAsync(item.Id, newPriority, token);

            if (!success)
            {
                MoveItemToGroup(item, previous);
                ShowSnackbar(error ?? "Error al actualizar prioridad");
                return false;
            }

            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                ShowSnackbar("Prioridad guardada localmente; se sincronizará al reconectar");

            return true;
        }

        public void MoveItemToGroup(TriagePatientItem item, PriorityLevel newPriority)
        {
            var oldGroup = Groups.FirstOrDefault(g => g.Any(i => i.Id == item.Id));
            if (oldGroup != null)
                oldGroup.Remove(item);

            item.Priority = newPriority;

            var target = Groups.FirstOrDefault(g => g.Level == newPriority);
            if (target != null)
                target.Add(item);
        }

        private void RebuildGroups(IEnumerable<PatientResponseDto> patients)
        {
            foreach (var group in Groups)
                group.Clear();

            foreach (var dto in patients)
            {
                var item = TriagePatientItem.FromDto(dto);
                var group = Groups.FirstOrDefault(g => g.Level == item.Priority);
                if (group != null)
                    group.Add(item);
            }
        }

        private void ClearGroups()
        {
            foreach (var group in Groups)
                group.Clear();
        }

        private async Task UpdateLastSyncTextAsync(bool fromCache)
        {
            if (IsOffline || fromCache)
            {
                try
                {
                    var raw = await SecureStorage.GetAsync(AppConstants.LastPatientSyncKey);
                    if (!string.IsNullOrEmpty(raw) && DateTime.TryParse(raw, out var sync))
                    {
                        LastSyncText = $"Modo sin conexión — última sincronización: {sync.ToLocalTime():dd/MM/yyyy HH:mm}";
                        return;
                    }
                }
                catch { }

                LastSyncText = "Modo sin conexión — datos locales";
                return;
            }

            LastSyncText = string.Empty;
        }

        private void ShowSnackbar(string message)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                SnackbarMessage = message;
                IsSnackbarVisible = true;
            });

            Device.StartTimer(TimeSpan.FromSeconds(3), () =>
            {
                Device.BeginInvokeOnMainThread(() => IsSnackbarVisible = false);
                return false;
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
