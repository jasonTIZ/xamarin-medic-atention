using Medical_atention.Constants;
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
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;

        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _emailError = string.Empty;
        private string _passwordError = string.Empty;
        private string _generalError = string.Empty;
        private bool _isLoading;

        public LoginViewModel() : this(new AuthService()) { }

        public LoginViewModel(IAuthService authService)
        {
            _authService = authService;
            LoginCommand = new Command(async () => await ExecuteLoginAsync(), () => !_isLoading);
        }

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); EmailError = string.Empty; }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); PasswordError = string.Empty; }
        }

        public string EmailError
        {
            get => _emailError;
            set { _emailError = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasEmailError)); }
        }

        public string PasswordError
        {
            get => _passwordError;
            set { _passwordError = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasPasswordError)); }
        }

        public string GeneralError
        {
            get => _generalError;
            set { _generalError = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasGeneralError)); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); ((Command)LoginCommand).ChangeCanExecute(); }
        }

        public bool HasEmailError   => !string.IsNullOrEmpty(_emailError);
        public bool HasPasswordError => !string.IsNullOrEmpty(_passwordError);
        public bool HasGeneralError  => !string.IsNullOrEmpty(_generalError);

        public ICommand LoginCommand { get; }

        private bool Validate()
        {
            bool valid = true;

            if (string.IsNullOrWhiteSpace(_email))
            {
                EmailError = "El email es requerido";
                valid = false;
            }

            if (string.IsNullOrWhiteSpace(_password))
            {
                PasswordError = "La contraseña es requerida";
                valid = false;
            }

            return valid;
        }

        private async Task ExecuteLoginAsync()
        {
            GeneralError = string.Empty;
            EmailError = string.Empty;
            PasswordError = string.Empty;

            if (!Validate()) return;

            IsLoading = true;
            try
            {
                var result = await _authService.LoginAsync(_email, _password);

                if (result == null)
                {
                    GeneralError = "Credenciales inválidas";
                    return;
                }

                await SecureStorage.SetAsync(AppConstants.TokenKey, result.Token);
                await SecureStorage.SetAsync(AppConstants.UserIdKey, result.User.Id.ToString());
                await SecureStorage.SetAsync(AppConstants.UserNameKey, result.User.Name);
                await SecureStorage.SetAsync(AppConstants.UserRoleKey, result.User.Role);

                Device.BeginInvokeOnMainThread(() =>
                {
                    Application.Current.MainPage = new Views.AppShell();
                });
            }
            catch (Exception)
            {
                GeneralError = "Sin conexión, verifica tu red";
            }
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
