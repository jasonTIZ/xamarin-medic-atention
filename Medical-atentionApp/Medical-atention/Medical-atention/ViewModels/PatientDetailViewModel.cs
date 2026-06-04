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

        public PatientDetailViewModel(int id) : this(id, new PatientService()) { }

        public PatientDetailViewModel(int id, IPatientService patientService)
        {
            _patientService = patientService;
            _id = id;
            EditCommand = new Command(StartEdit, () => !IsLoading && !IsEditing);
            SaveCommand = new Command(async () => await SaveAsync(), () => !IsSaving);
            CancelCommand = new Command(CancelEdit, () => IsEditing);
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

        private void ApplyPatient(PatientResponseDto patient)
        {
            _id = patient.Id;
            Name = patient.Name;
            LastName = patient.LastName;
            IdentificationNumber = patient.IdentificationNumber;
            DateOfBirth = patient.DateOfBirth;
            Gender = patient.Gender;
            SelectedGender = patient.Gender;
            CreatedAt = patient.CreatedAt;
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
