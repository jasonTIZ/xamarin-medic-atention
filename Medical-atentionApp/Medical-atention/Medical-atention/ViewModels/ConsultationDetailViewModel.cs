using Medical_atention.Constants;
using Medical_atention.Services;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Medical_atention.ViewModels
{
    public class ConsultationDetailViewModel : INotifyPropertyChanged
    {
        private readonly IConsultationService _consultationService;
        private static readonly CultureInfo EsCulture = CreateSpanishCulture();

        private static CultureInfo CreateSpanishCulture()
        {
            try { return new CultureInfo("es-ES"); }
            catch { return CultureInfo.CurrentCulture; }
        }

        private string _dateDisplay = string.Empty;
        private string _diagnosis = string.Empty;
        private string _symptoms = string.Empty;
        private string _treatment = string.Empty;
        private string _notes = string.Empty;
        private string _priority = "medium";
        private bool _isLoading;
        private bool _pendingSync;

        public ConsultationDetailViewModel() : this(new ConsultationService()) { }

        public ConsultationDetailViewModel(IConsultationService consultationService)
        {
            _consultationService = consultationService;
        }

        public string DateDisplay
        {
            get => _dateDisplay;
            set { _dateDisplay = value; OnPropertyChanged(); }
        }

        public string Diagnosis
        {
            get => _diagnosis;
            set { _diagnosis = value; OnPropertyChanged(); }
        }

        public string Symptoms
        {
            get => _symptoms;
            set { _symptoms = value; OnPropertyChanged(); }
        }

        public string Treatment
        {
            get => _treatment;
            set { _treatment = value; OnPropertyChanged(); }
        }

        public string Notes
        {
            get => _notes;
            set { _notes = value; OnPropertyChanged(); }
        }

        public string Priority
        {
            get => _priority;
            set { _priority = value; OnPropertyChanged(); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNotLoading)); }
        }

        public bool IsNotLoading => !_isLoading;

        public bool PendingSync
        {
            get => _pendingSync;
            set { _pendingSync = value; OnPropertyChanged(); }
        }

        public async Task LoadAsync(int? serverId, int localId)
        {
            IsLoading = true;
            try
            {
                var detail = await _consultationService.GetDetailAsync(
                    serverId > 0 ? serverId : null,
                    localId,
                    await SecureStorage.GetAsync(AppConstants.TokenKey));

                if (detail is null) return;

                DateDisplay = detail.ConsultationDate.ToString("dd MMM, yyyy - HH:mm", EsCulture);
                Diagnosis = detail.Diagnosis ?? string.Empty;
                Symptoms = detail.Symptoms ?? string.Empty;
                Treatment = string.IsNullOrWhiteSpace(detail.Treatment) ? "—" : detail.Treatment;
                Notes = string.IsNullOrWhiteSpace(detail.Notes) ? "—" : detail.Notes;
                Priority = detail.Priority ?? "medium";
            }
            catch (Exception) { }
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
