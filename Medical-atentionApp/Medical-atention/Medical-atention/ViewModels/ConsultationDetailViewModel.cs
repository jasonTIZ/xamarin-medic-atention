using Medical_atention.Constants;
using Medical_atention.Models;
using Medical_atention.Services;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Medical_atention.ViewModels
{
    public class ConsultationDetailViewModel : INotifyPropertyChanged
    {
        private readonly IConsultationService _consultationService;
        private static readonly CultureInfo EsCulture = CreateSpanishCulture();

        private int? _serverId;
        private int _localId;
        private string _dateDisplay = string.Empty;
        private string _diagnosis = string.Empty;
        private string _symptoms = string.Empty;
        private string _treatment = string.Empty;
        private string _notes = string.Empty;
        private string _priority = "medium";
        private string _backupTreatment = string.Empty;
        private string _backupNotes = string.Empty;
        private bool _isLoading;
        private bool _isEditing;
        private bool _isSaving;
        private string _errorMessage = string.Empty;

        public ConsultationDetailViewModel() : this(new ConsultationService()) { }

        public ConsultationDetailViewModel(IConsultationService consultationService)
        {
            _consultationService = consultationService;
            EditCommand = new Command(StartEdit, () => !IsLoading && !IsEditing);
            SaveCommand = new Command(async () => await SaveAsync(), () => !IsSaving);
            CancelCommand = new Command(CancelEdit, () => IsEditing);
        }

        private static CultureInfo CreateSpanishCulture()
        {
            try { return new CultureInfo("es-ES"); }
            catch { return CultureInfo.CurrentCulture; }
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
            set { _treatment = value; OnPropertyChanged(); OnPropertyChanged(nameof(TreatmentReadOnly)); }
        }

        public string Notes
        {
            get => _notes;
            set { _notes = value; OnPropertyChanged(); OnPropertyChanged(nameof(NotesReadOnly)); }
        }

        public string TreatmentReadOnly => ToReadOnlyDisplay(_treatment);

        public string NotesReadOnly => ToReadOnlyDisplay(_notes);

        public string Priority
        {
            get => _priority;
            set { _priority = value; OnPropertyChanged(); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotLoading));
                OnPropertyChanged(nameof(ShowEditButton));
                ((Command)EditCommand).ChangeCanExecute();
            }
        }

        public bool IsNotLoading => !_isLoading;

        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                _isEditing = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsReadOnly));
                OnPropertyChanged(nameof(ShowEditButton));
                OnPropertyChanged(nameof(ShowSaveCancel));
                OnPropertyChanged(nameof(TreatmentReadOnly));
                OnPropertyChanged(nameof(NotesReadOnly));
                ((Command)EditCommand).ChangeCanExecute();
                ((Command)CancelCommand).ChangeCanExecute();
            }
        }

        public bool IsReadOnly => !_isEditing;
        public bool ShowEditButton => !_isEditing && IsNotLoading;
        public bool ShowSaveCancel => _isEditing;

        public bool IsSaving
        {
            get => _isSaving;
            set
            {
                _isSaving = value;
                OnPropertyChanged();
                ((Command)SaveCommand).ChangeCanExecute();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasError)); }
        }

        public bool HasError => !string.IsNullOrEmpty(_errorMessage);

        public ICommand EditCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public async Task LoadAsync(int? serverId, int localId)
        {
            _serverId = serverId > 0 ? serverId : null;
            _localId = localId;

            IsLoading = true;
            ErrorMessage = string.Empty;
            try
            {
                var detail = await _consultationService.GetDetailAsync(
                    _serverId, _localId, await SecureStorage.GetAsync(AppConstants.TokenKey));

                if (detail is null)
                {
                    ErrorMessage = "No se pudo cargar la consulta";
                    return;
                }

                ApplyDetail(detail);
            }
            catch (Exception)
            {
                ErrorMessage = "Sin conexión, verifica tu red";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ApplyDetail(ConsultationResponseDto detail)
        {
            DateDisplay = detail.ConsultationDate.ToString("dd MMM, yyyy - HH:mm", EsCulture);
            Diagnosis = detail.Diagnosis ?? string.Empty;
            Symptoms = detail.Symptoms ?? string.Empty;
            Treatment = detail.Treatment ?? string.Empty;
            Notes = detail.Notes ?? string.Empty;
            Priority = detail.Priority ?? "medium";
            OnPropertyChanged(nameof(ShowEditButton));
        }

        private void StartEdit()
        {
            _backupTreatment = _treatment;
            _backupNotes = _notes;
            ErrorMessage = string.Empty;
            IsEditing = true;
        }

        private void CancelEdit()
        {
            Treatment = _backupTreatment;
            Notes = _backupNotes;
            ErrorMessage = string.Empty;
            IsEditing = false;
        }

        private async Task SaveAsync()
        {
            ErrorMessage = string.Empty;
            IsSaving = true;
            try
            {
                var request = new ConsultationUpdateDto
                {
                    Treatment = _treatment?.Trim() ?? string.Empty,
                    Notes = _notes?.Trim() ?? string.Empty
                };

                var (success, error) = await _consultationService.UpdateAsync(
                    _serverId, _localId, request, await SecureStorage.GetAsync(AppConstants.TokenKey));

                if (!success)
                {
                    ErrorMessage = error ?? "No se pudo guardar los cambios";
                    return;
                }

                Device.BeginInvokeOnMainThread(() => IsEditing = false);
            }
            catch (Exception)
            {
                ErrorMessage = "Sin conexión, verifica tu red";
            }
            finally
            {
                Device.BeginInvokeOnMainThread(() => IsSaving = false);
            }
        }

        private static string ToReadOnlyDisplay(string value)
            => string.IsNullOrWhiteSpace(value) ? "—" : value.Trim();

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
