using Medical_atention.Constants;
using Medical_atention.Helpers;
using Medical_atention.Services;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Medical_atention.ViewModels
{
    public class ProfileViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;

        private string _userName;
        private string _userRole;
        private string _userEmail;
        private string _currentPassword;
        private string _newPassword;
        private string _confirmPassword;
        private string _currentPasswordError;
        private string _newPasswordError;
        private string _confirmPasswordError;
        private string _successMessage;
        private bool _isLoading;

        public ProfileViewModel() : this(new AuthService()) { }

        public ProfileViewModel(IAuthService authService)
        {
            _authService = authService;
            ChangePasswordCommand = new Command(async () => await ExecuteChangePasswordAsync(), () => !_isLoading);
            LogoutCommand = new Command(async () => await ExecuteLogoutAsync());
            _ = LoadUserInfoAsync();
        }

        public string UserName
        {
            get => _userName;
            set { _userName = value; OnPropertyChanged(); }
        }

        public string UserRole
        {
            get => _userRole;
            set { _userRole = value; OnPropertyChanged(); }
        }

        public string UserEmail
        {
            get => _userEmail;
            set { _userEmail = value; OnPropertyChanged(); }
        }

        public string CurrentPassword
        {
            get => _currentPassword;
            set { _currentPassword = value; OnPropertyChanged(); CurrentPasswordError = string.Empty; }
        }

        public string NewPassword
        {
            get => _newPassword;
            set { _newPassword = value; OnPropertyChanged(); NewPasswordError = string.Empty; }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set { _confirmPassword = value; OnPropertyChanged(); ConfirmPasswordError = string.Empty; }
        }

        public string CurrentPasswordError
        {
            get => _currentPasswordError;
            set { _currentPasswordError = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasCurrentPasswordError)); }
        }

        public string NewPasswordError
        {
            get => _newPasswordError;
            set { _newPasswordError = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasNewPasswordError)); }
        }

        public string ConfirmPasswordError
        {
            get => _confirmPasswordError;
            set { _confirmPasswordError = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasConfirmPasswordError)); }
        }

        public string SuccessMessage
        {
            get => _successMessage;
            set { _successMessage = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasSuccessMessage)); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); ((Command)ChangePasswordCommand).ChangeCanExecute(); }
        }

        public bool HasCurrentPasswordError => !string.IsNullOrEmpty(_currentPasswordError);
        public bool HasNewPasswordError => !string.IsNullOrEmpty(_newPasswordError);
        public bool HasConfirmPasswordError => !string.IsNullOrEmpty(_confirmPasswordError);
        public bool HasSuccessMessage => !string.IsNullOrEmpty(_successMessage);

        public ICommand ChangePasswordCommand { get; }
        public ICommand LogoutCommand { get; }

        private async Task LoadUserInfoAsync()
        {
            try
            {
                UserName = await SecureStorage.GetAsync(AppConstants.UserNameKey) ?? "Usuario";
                UserRole = await SecureStorage.GetAsync(AppConstants.UserRoleKey) ?? string.Empty;

                var token = await SecureStorage.GetAsync(AppConstants.TokenKey);
                // Email is embedded in the JWT; ClaimTypes.Email maps to the long URI claim key
                UserEmail = JwtHelper.GetClaim(token, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")
                         ?? JwtHelper.GetClaim(token, "email")
                         ?? string.Empty;
            }
            catch (Exception) { }
        }

        private bool Validate()
        {
            bool valid = true;

            if (string.IsNullOrWhiteSpace(_currentPassword))
            {
                CurrentPasswordError = "Ingresa tu contraseña actual";
                valid = false;
            }

            if (string.IsNullOrWhiteSpace(_newPassword) || _newPassword.Length < 8)
            {
                NewPasswordError = "La nueva contraseña debe tener mínimo 8 caracteres";
                valid = false;
            }

            if (_newPassword != _confirmPassword)
            {
                ConfirmPasswordError = "Las contraseñas no coinciden";
                valid = false;
            }

            return valid;
        }

        private async Task ExecuteChangePasswordAsync()
        {
            CurrentPasswordError = string.Empty;
            NewPasswordError = string.Empty;
            ConfirmPasswordError = string.Empty;
            SuccessMessage = string.Empty;

            if (!Validate()) return;

            IsLoading = true;
            try
            {
                var token = await SecureStorage.GetAsync(AppConstants.TokenKey);
                var success = await _authService.ChangePasswordAsync(token, _currentPassword, _newPassword);

                if (!success)
                {
                    CurrentPasswordError = "La contraseña actual es incorrecta";
                    return;
                }

                CurrentPassword = string.Empty;
                NewPassword = string.Empty;
                ConfirmPassword = string.Empty;

                SuccessMessage = "Contraseña actualizada";
                _ = HideSuccessMessageAfterDelayAsync();
            }
            catch (Exception)
            {
                CurrentPasswordError = "Sin conexión, verifica tu red";
            }
            finally
            {
                Device.BeginInvokeOnMainThread(() => IsLoading = false);
            }
        }

        private async Task HideSuccessMessageAfterDelayAsync()
        {
            await Task.Delay(3000);
            Device.BeginInvokeOnMainThread(() => SuccessMessage = string.Empty);
        }

        private async Task ExecuteLogoutAsync()
        {
            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Cerrar sesión", "¿Deseas cerrar sesión?", "Sí", "Cancelar");
            if (!confirm) return;

            SecureStorage.RemoveAll();
            Device.BeginInvokeOnMainThread(() =>
            {
                Application.Current.MainPage = new Views.LoginPage();
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
