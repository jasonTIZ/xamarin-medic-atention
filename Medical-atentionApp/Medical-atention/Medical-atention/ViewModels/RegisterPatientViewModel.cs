using Medical_atention.Constants;
using Medical_atention.Models;
using Medical_atention.Services;
using Medical_atention.Views;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Medical_atention.ViewModels
{
    public class RegisterPatientViewModel : INotifyPropertyChanged
    {
        private readonly IPatientService _patientService;
        private static readonly Regex IdentificationNumberRegex = new Regex(@"^\d{9,12}$");

        private string _name = string.Empty;
        private string _lastName = string.Empty;
        private string _identificationNumber = string.Empty;
        private DateTime _dateOfBirth = new DateTime(1990, 1, 1);
        private string _selectedGender = string.Empty;
        private string _nameError = string.Empty;
        private string _lastNameError = string.Empty;
        private string _identificationNumberError = string.Empty;
        private string _dateError = string.Empty;
        private string _genderError = string.Empty;
        private string _generalError = string.Empty;
        private string _snackbarMessage = string.Empty;
        private bool _isLoading;
        private bool _isSnackbarVisible;

        public RegisterPatientViewModel() : this(new PatientService()) { }

        public RegisterPatientViewModel(IPatientService patientService)
        {
            _patientService = patientService;
            RegisterCommand = new Command(async () => await ExecuteRegisterAsync(), () => !_isLoading);
        }

        public string[] Genders { get; } = { "Masculino", "Femenino", "Otro" };
        public DateTime MaxDateOfBirth => DateTime.Today.AddDays(-1);

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); NameError = string.Empty; }
        }

        public string LastName
        {
            get => _lastName;
            set { _lastName = value; OnPropertyChanged(); LastNameError = string.Empty; }
        }

        public string IdentificationNumber
        {
            get => _identificationNumber;
            set { _identificationNumber = value; OnPropertyChanged(); IdentificationNumberError = string.Empty; }
        }

        public DateTime DateOfBirth
        {
            get => _dateOfBirth;
            set { _dateOfBirth = value; OnPropertyChanged(); DateError = string.Empty; }
        }

        public string SelectedGender
        {
            get => _selectedGender;
            set { _selectedGender = value ?? string.Empty; OnPropertyChanged(); GenderError = string.Empty; }
        }

        public string NameError
        {
            get => _nameError;
            set { _nameError = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasNameError)); }
        }

        public string LastNameError
        {
            get => _lastNameError;
            set { _lastNameError = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasLastNameError)); }
        }

        public string IdentificationNumberError
        {
            get => _identificationNumberError;
            set { _identificationNumberError = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasIdentificationNumberError)); }
        }

        public string DateError
        {
            get => _dateError;
            set { _dateError = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasDateError)); }
        }

        public string GenderError
        {
            get => _genderError;
            set { _genderError = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasGenderError)); }
        }

        public string GeneralError
        {
            get => _generalError;
            set { _generalError = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasGeneralError)); }
        }

        public string SnackbarMessage
        {
            get => _snackbarMessage;
            set { _snackbarMessage = value; OnPropertyChanged(); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); ((Command)RegisterCommand).ChangeCanExecute(); }
        }

        public bool IsSnackbarVisible
        {
            get => _isSnackbarVisible;
            set { _isSnackbarVisible = value; OnPropertyChanged(); }
        }

        public bool HasNameError     => !string.IsNullOrEmpty(_nameError);
        public bool HasLastNameError => !string.IsNullOrEmpty(_lastNameError);
        public bool HasIdentificationNumberError   => !string.IsNullOrEmpty(_identificationNumberError);
        public bool HasDateError    => !string.IsNullOrEmpty(_dateError);
        public bool HasGenderError  => !string.IsNullOrEmpty(_genderError);
        public bool HasGeneralError => !string.IsNullOrEmpty(_generalError);

        public ICommand RegisterCommand { get; }

        private bool Validate()
        {
            bool valid = true;

            if (string.IsNullOrWhiteSpace(_name))
            {
                NameError = "El nombre es requerido";
                valid = false;
            }

            if (string.IsNullOrWhiteSpace(_lastName))
            {
                LastNameError = "El apellido es requerido";
                valid = false;
            }

            if (string.IsNullOrWhiteSpace(_identificationNumber) || !IdentificationNumberRegex.IsMatch(_identificationNumber))
            {
                IdentificationNumberError = "Formato inválido (9 a 12 dígitos)";
                valid = false;
            }

            if (_dateOfBirth.Date >= DateTime.Today)
            {
                DateError = "La fecha no puede ser futura";
                valid = false;
            }

            if (string.IsNullOrEmpty(_selectedGender))
            {
                GenderError = "Selecciona un género";
                valid = false;
            }

            return valid;
        }

        private async Task ExecuteRegisterAsync()
        {
            GeneralError = string.Empty;
            if (!Validate()) return;

            IsLoading = true;
            try
            {
                var request = new PatientRequestDto
                {
                    Name = _name.Trim(),
                    LastName = _lastName.Trim(),
                    IdentificationNumber = _identificationNumber.Trim(),
                    DateOfBirth = _dateOfBirth,
                    Gender = _selectedGender
                };

                var token = await SecureStorage.GetAsync(AppConstants.TokenKey);
                int patientId;

                if (_patientService.IsOnline() && !string.IsNullOrEmpty(token))
                {
                    var (dto, error) = await _patientService.RegisterAsync(request, token);
                    if (error != null)
                    {
                        if (error.Contains("cédula"))
                            IdentificationNumberError = error;
                        else
                            GeneralError = error;
                        return;
                    }
                    patientId = dto.Id;
                }
                else
                {
                    var (local, error) = await _patientService.RegisterLocalAsync(request);
                    if (error != null)
                    {
                        GeneralError = error;
                        return;
                    }
                    patientId = local.Id;
                }

                Device.BeginInvokeOnMainThread(() =>
                {
                    SnackbarMessage = "Paciente registrado exitosamente.";
                    IsSnackbarVisible = true;
                });
                Device.StartTimer(TimeSpan.FromSeconds(2), () =>
                {
                    Device.BeginInvokeOnMainThread(async () =>
                        await Shell.Current.GoToAsync($"PatientDetailPage?id={patientId}"));
                    return false;
                });
            }
            catch (Exception)
            {
                GeneralError = "Sin conexión, verifica tu red";
            }
            finally
            {
                Device.BeginInvokeOnMainThread(() => IsLoading = false);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
