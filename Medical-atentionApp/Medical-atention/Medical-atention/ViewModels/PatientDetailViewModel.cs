using Medical_atention.Constants;
using Medical_atention.Models;
using Medical_atention.Services;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Medical_atention.ViewModels
{
    public class PatientDetailViewModel : INotifyPropertyChanged
    {
        private readonly IPatientService _patientService;
        private readonly INotificationService _notificationService;
        private static readonly Regex IdentificationNumberRegex = new Regex(@"^\d{9,12}$");

        private int _id;
        private string _name = string.Empty;
        private string _lastName = string.Empty;
        private string _identificationNumber = string.Empty;
        private DateTime _dateOfBirth;
        private string _gender = string.Empty;
        private DateTime _createdAt;
        private string _selectedGender = string.Empty;
        private bool _isLoading;
        private bool _isEditing;
        private bool _isSaving;
        private string _errorMessage = string.Empty;

        private string _backupName;
        private string _backupLastName;
        private string _backupIdentificationNumber;
        private DateTime _backupDateOfBirth;
        private string _backupGender;

        private int _pendingReminderCount;
        private bool _isCancellingReminders;
        private string _reminderMessage = string.Empty;

        public PatientDetailViewModel(int id) : this(id, new PatientService(), new NotificationService()) { }

        public PatientDetailViewModel(int id, IPatientService patientService)
            : this(id, patientService, new NotificationService()) { }

        public int PatientId => _id;

        public PatientDetailViewModel(int id, IPatientService patientService, INotificationService notificationService)
        {
            _patientService = patientService;
            _notificationService = notificationService;
            _id = id;
            EditCommand = new Command(StartEdit, () => !IsLoading && !IsEditing);
            SaveCommand = new Command(async () => await SaveAsync(), () => !IsSaving);
            CancelCommand = new Command(CancelEdit, () => IsEditing);
            CancelRemindersCommand = new Command(
                async () => await CancelRemindersAsync(),
                () => HasPendingReminders && !_isCancellingReminders);
            _ = LoadPatientAsync(id);
        }

        public string[] Genders { get; } = { "Masculino", "Femenino", "Otro" };
        public DateTime MaxDateOfBirth => DateTime.Today.AddDays(-1);

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string LastName
        {
            get => _lastName;
            set { _lastName = value; OnPropertyChanged(); }
        }

        public string IdentificationNumber
        {
            get => _identificationNumber;
            set { _identificationNumber = value; OnPropertyChanged(); }
        }

        public DateTime DateOfBirth
        {
            get => _dateOfBirth;
            set { _dateOfBirth = value; OnPropertyChanged(); }
        }

        public string Gender
        {
            get => _gender;
            set { _gender = value; OnPropertyChanged(); }
        }

        public string SelectedGender
        {
            get => _selectedGender;
            set { _selectedGender = value ?? string.Empty; OnPropertyChanged(); Gender = _selectedGender; }
        }

        public DateTime CreatedAt
        {
            get => _createdAt;
            set { _createdAt = value; OnPropertyChanged(); }
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
        public ICommand CancelRemindersCommand { get; }

        public int PendingReminderCount
        {
            get => _pendingReminderCount;
            set
            {
                _pendingReminderCount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasPendingReminders));
                OnPropertyChanged(nameof(ReminderInfoText));
                ((Command)CancelRemindersCommand).ChangeCanExecute();
            }
        }

        public bool HasPendingReminders => _pendingReminderCount > 0;

        public string ReminderInfoText => _pendingReminderCount == 1
            ? "1 recordatorio de seguimiento programado"
            : $"{_pendingReminderCount} recordatorios de seguimiento programados";

        public string ReminderMessage
        {
            get => _reminderMessage;
            set { _reminderMessage = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasReminderMessage)); }
        }

        public bool HasReminderMessage => !string.IsNullOrEmpty(_reminderMessage);

        private async Task LoadPatientAsync(int id)
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            try
            {
                var patient = await _patientService.GetPatientAsync(id, await SecureStorage.GetAsync(AppConstants.TokenKey));
                if (patient is null)
                {
                    ErrorMessage = "No se pudo cargar el paciente";
                    return;
                }

                Device.BeginInvokeOnMainThread(() => ApplyPatient(patient));
                await LoadRemindersAsync();
            }
            catch (Exception)
            {
                Device.BeginInvokeOnMainThread(() => ErrorMessage = "Sin conexión, verifica tu red");
            }
            finally
            {
                Device.BeginInvokeOnMainThread(() => IsLoading = false);
            }
        }

        private async Task LoadRemindersAsync()
        {
            try
            {
                var count = await _notificationService.GetPendingCountAsync(_id);
                Device.BeginInvokeOnMainThread(() => PendingReminderCount = count);
            }
            catch (Exception)
            {
                // Sin recordatorios disponibles; se omite silenciosamente.
            }
        }

        private async Task CancelRemindersAsync()
        {
            _isCancellingReminders = true;
            Device.BeginInvokeOnMainThread(() => ((Command)CancelRemindersCommand).ChangeCanExecute());
            try
            {
                var cancelled = await _notificationService.CancelForPatientAsync(_id);
                Device.BeginInvokeOnMainThread(() =>
                {
                    PendingReminderCount = 0;
                    ReminderMessage = cancelled > 0
                        ? $"{cancelled} recordatorio(s) de seguimiento cancelado(s)"
                        : "No había recordatorios pendientes";
                });
            }
            catch (Exception)
            {
                Device.BeginInvokeOnMainThread(() =>
                    ReminderMessage = "No se pudieron cancelar los recordatorios");
            }
            finally
            {
                _isCancellingReminders = false;
                Device.BeginInvokeOnMainThread(() => ((Command)CancelRemindersCommand).ChangeCanExecute());
            }
        }

        private void ApplyPatient(PatientResponseDto patient)
        {
            _id = patient.Id;
            Name = patient.Name ?? string.Empty;
            LastName = patient.LastName ?? string.Empty;
            IdentificationNumber = patient.IdentificationNumber ?? string.Empty;
            DateOfBirth = patient.DateOfBirth == default ? new DateTime(1990, 1, 1) : patient.DateOfBirth;
            Gender = patient.Gender ?? string.Empty;
            SelectedGender = patient.Gender ?? string.Empty;
            CreatedAt = patient.CreatedAt;
            OnPropertyChanged(nameof(ShowEditButton));
        }

        private void StartEdit()
        {
            _backupName = Name;
            _backupLastName = LastName;
            _backupIdentificationNumber = IdentificationNumber;
            _backupDateOfBirth = DateOfBirth;
            _backupGender = Gender;
            SelectedGender = Gender;
            ErrorMessage = string.Empty;
            IsEditing = true;
        }

        private void CancelEdit()
        {
            Name = _backupName;
            LastName = _backupLastName;
            IdentificationNumber = _backupIdentificationNumber;
            DateOfBirth = _backupDateOfBirth;
            Gender = _backupGender;
            SelectedGender = _backupGender;
            ErrorMessage = string.Empty;
            IsEditing = false;
        }

        private bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(LastName))
            {
                ErrorMessage = "Nombre y apellido son requeridos";
                return false;
            }

            if (string.IsNullOrWhiteSpace(IdentificationNumber) || !IdentificationNumberRegex.IsMatch(IdentificationNumber))
            {
                ErrorMessage = "Cédula inválida (9 a 12 dígitos)";
                return false;
            }

            if (DateOfBirth.Date >= DateTime.Today)
            {
                ErrorMessage = "La fecha de nacimiento no puede ser futura";
                return false;
            }

            if (string.IsNullOrEmpty(SelectedGender))
            {
                ErrorMessage = "Selecciona un género";
                return false;
            }

            return true;
        }

        private async Task SaveAsync()
        {
            ErrorMessage = string.Empty;
            if (!Validate()) return;

            IsSaving = true;
            try
            {
                var request = new PatientRequestDto
                {
                    Name = Name.Trim(),
                    LastName = LastName.Trim(),
                    IdentificationNumber = IdentificationNumber.Trim(),
                    DateOfBirth = DateOfBirth,
                    Gender = SelectedGender
                };

                var (patient, error) = await _patientService.UpdateAsync(
                    _id, request, await SecureStorage.GetAsync(AppConstants.TokenKey));

                if (error != null)
                {
                    ErrorMessage = error;
                    return;
                }

                Device.BeginInvokeOnMainThread(() =>
                {
                    ApplyPatient(patient);
                    IsEditing = false;
                });
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
