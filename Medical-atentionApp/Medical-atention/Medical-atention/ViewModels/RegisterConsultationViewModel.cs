using Medical_atention.Constants;
using Medical_atention.Data;
using Medical_atention.Models;
using Medical_atention.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Medical_atention.ViewModels
{
    public class RegisterConsultationViewModel : INotifyPropertyChanged
    {
        private readonly IConsultationService _consultationService;
        private readonly IAttachmentService _attachmentService;
        private readonly IConnectivityService _connectivity;

        private readonly List<PendingImage> _pendingImages = new List<PendingImage>();
        private ObservableCollection<PendingImageItem> _previewImages = new ObservableCollection<PendingImageItem>();
        private string _attachmentError = string.Empty;

        private int _patientId;
        private string _patientName = string.Empty;
        private string _patientCedula = string.Empty;
        private bool _isEditingDateTime;
        private string _symptoms = string.Empty;
        private string _diagnosis = string.Empty;
        private string _treatment = string.Empty;
        private string _notes = string.Empty;
        private string _selectedPriority = "medium";
        private DateTime _consultationDate = DateTime.Today;
        private TimeSpan _consultationTime = DateTime.Now.TimeOfDay;
        private string _symptomsError = string.Empty;
        private string _diagnosisError = string.Empty;
        private string _generalError = string.Empty;
        private string _snackbarMessage = string.Empty;
        private bool _isLoading;
        private bool _isSnackbarVisible;
        private bool _showPendingSync;

        public RegisterConsultationViewModel() : this(new ConsultationService(), new AttachmentService(), ConnectivityService.Instance) { }

        public RegisterConsultationViewModel(
            IConsultationService consultationService,
            IAttachmentService attachmentService,
            IConnectivityService connectivity = null)
        {
            _consultationService = consultationService;
            _attachmentService = attachmentService;
            _connectivity = connectivity ?? ConnectivityService.Instance;
            var selectPriority = new Command<string>(SelectPriority);
            PriorityOptions = new ObservableCollection<PriorityOptionItem>
            {
                new PriorityOptionItem("low", selectPriority),
                new PriorityOptionItem("medium", selectPriority),
                new PriorityOptionItem("high", selectPriority),
                new PriorityOptionItem("urgent", selectPriority)
            };
            SelectPriority("medium");
            RegisterCommand = new Command(async () => await ExecuteRegisterAsync(), () => !_isLoading);
            ToggleEditDateTimeCommand = new Command(ToggleEditDateTime);
            AddFromGalleryCommand = new Command(async () => await PickImageAsync(fromCamera: false), () => CanAddImage);
            AddFromCameraCommand = new Command(async () => await PickImageAsync(fromCamera: true), () => CanAddImage);
            RemoveImageCommand = new Command<PendingImageItem>(RemoveImage);
            UpdateOfflineIndicator();
            _connectivity.Start();
            Connectivity.ConnectivityChanged += (_, __) => UpdateOfflineIndicator();
        }

        public string PatientDisplayLine =>
            string.IsNullOrWhiteSpace(_patientCedula)
                ? $"Para el paciente: {_patientName}"
                : $"Para el paciente: {_patientName}, Cédula: {_patientCedula}";

        public string ConsultationDateTimeDisplay => ConsultationDateTime.ToString("yyyy-MM-dd HH:mm");

        public bool IsEditingDateTime
        {
            get => _isEditingDateTime;
            set
            {
                _isEditingDateTime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsDateTimeReadOnly));
            }
        }

        public bool IsDateTimeReadOnly => !_isEditingDateTime;

        public ObservableCollection<PriorityOptionItem> PriorityOptions { get; }

        public string PatientName
        {
            get => _patientName;
            set { _patientName = value; OnPropertyChanged(); OnPropertyChanged(nameof(PatientDisplayLine)); }
        }

        public string Symptoms
        {
            get => _symptoms;
            set { _symptoms = value; OnPropertyChanged(); SymptomsError = string.Empty; }
        }

        public string Diagnosis
        {
            get => _diagnosis;
            set { _diagnosis = value; OnPropertyChanged(); DiagnosisError = string.Empty; }
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

        public DateTime ConsultationDate
        {
            get => _consultationDate;
            set
            {
                _consultationDate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ConsultationDateTime));
                OnPropertyChanged(nameof(ConsultationDateTimeDisplay));
            }
        }

        public TimeSpan ConsultationTime
        {
            get => _consultationTime;
            set
            {
                _consultationTime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ConsultationDateTime));
                OnPropertyChanged(nameof(ConsultationDateTimeDisplay));
            }
        }

        public DateTime ConsultationDateTime => _consultationDate.Date.Add(_consultationTime);

        public string SymptomsError
        {
            get => _symptomsError;
            set { _symptomsError = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasSymptomsError)); }
        }

        public string DiagnosisError
        {
            get => _diagnosisError;
            set { _diagnosisError = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasDiagnosisError)); }
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

        public bool ShowOfflineIndicator { get; private set; }

        public bool ShowPendingSync
        {
            get => _showPendingSync;
            set { _showPendingSync = value; OnPropertyChanged(); UpdateOfflineIndicator(); }
        }

        public ObservableCollection<PendingImageItem> PreviewImages
        {
            get => _previewImages;
            private set { _previewImages = value; OnPropertyChanged(); }
        }

        public string AttachmentError
        {
            get => _attachmentError;
            set { _attachmentError = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasAttachmentError)); }
        }

        public bool HasAttachmentError => !string.IsNullOrEmpty(_attachmentError);
        public bool CanAddImage => _pendingImages.Count < 3;
        public string ImageCountLabel => $"{_pendingImages.Count}/3 imágenes";
        public bool HasImages => _pendingImages.Count > 0;

        public bool HasSymptomsError => !string.IsNullOrEmpty(_symptomsError);
        public bool HasDiagnosisError => !string.IsNullOrEmpty(_diagnosisError);
        public bool HasGeneralError => !string.IsNullOrEmpty(_generalError);

        public ICommand RegisterCommand { get; }
        public ICommand ToggleEditDateTimeCommand { get; }
        public ICommand AddFromGalleryCommand { get; }
        public ICommand AddFromCameraCommand { get; }
        public ICommand RemoveImageCommand { get; }

        public void Initialize(int patientId, string patientName, string patientCedula = null)
        {
            _patientId = patientId;
            PatientName = patientName ?? string.Empty;
            _patientCedula = patientCedula ?? string.Empty;
            OnPropertyChanged(nameof(PatientDisplayLine));
            ConsultationDate = DateTime.Today;
            ConsultationTime = DateTime.Now.TimeOfDay;
            IsEditingDateTime = false;
        }

        private void ToggleEditDateTime() => IsEditingDateTime = !IsEditingDateTime;

        private async Task PickImageAsync(bool fromCamera)
        {
            AttachmentError = string.Empty;
            var (bytes, fileName, error) = await _attachmentService.PickAndCompressAsync(fromCamera);

            if (bytes is null)
            {
                if (!string.IsNullOrEmpty(error)) AttachmentError = error;
                return;
            }

            var preview = new PendingImageItem
            {
                FileName = fileName,
                Source = ImageSource.FromStream(() => new MemoryStream(bytes))
            };

            _pendingImages.Add(new PendingImage { Bytes = bytes, FileName = fileName });
            PreviewImages.Add(preview);

            OnPropertyChanged(nameof(CanAddImage));
            OnPropertyChanged(nameof(ImageCountLabel));
            OnPropertyChanged(nameof(HasImages));
            ((Command)AddFromGalleryCommand).ChangeCanExecute();
            ((Command)AddFromCameraCommand).ChangeCanExecute();
        }

        private void RemoveImage(PendingImageItem item)
        {
            if (item is null) return;
            var idx = PreviewImages.IndexOf(item);
            if (idx < 0) return;

            PreviewImages.RemoveAt(idx);
            if (idx < _pendingImages.Count)
                _pendingImages.RemoveAt(idx);

            OnPropertyChanged(nameof(CanAddImage));
            OnPropertyChanged(nameof(ImageCountLabel));
            OnPropertyChanged(nameof(HasImages));
            ((Command)AddFromGalleryCommand).ChangeCanExecute();
            ((Command)AddFromCameraCommand).ChangeCanExecute();
        }

        private async Task UploadPendingImagesAsync(int localId, int? serverId, string token)
        {
            foreach (var img in _pendingImages)
            {
                try
                {
                    await _attachmentService.AddBytesAsync(localId, serverId, img.Bytes, img.FileName, token, null);
                }
                catch (Exception) { }
            }
        }

        private void UpdateOfflineIndicator()
        {
            ShowOfflineIndicator = !_connectivity.IsConnected || _showPendingSync;
            OnPropertyChanged(nameof(ShowOfflineIndicator));
        }

        private void SelectPriority(string priority)
        {
            _selectedPriority = priority;
            foreach (var option in PriorityOptions)
                option.IsSelected = option.Key == priority;
        }

        private bool Validate()
        {
            var valid = true;

            if (string.IsNullOrWhiteSpace(_symptoms))
            {
                SymptomsError = "Los síntomas son requeridos";
                valid = false;
            }

            if (string.IsNullOrWhiteSpace(_diagnosis))
            {
                DiagnosisError = "El diagnóstico es requerido";
                valid = false;
            }

            return valid;
        }

        private async Task ExecuteRegisterAsync()
        {
            GeneralError = string.Empty;
            ShowPendingSync = false;
            if (!Validate()) return;

            IsLoading = true;
            try
            {
                var request = new ConsultationRequestDto
                {
                    PatientId = _patientId,
                    ConsultationDate = ConsultationDateTime,
                    Symptoms = _symptoms.Trim(),
                    Diagnosis = _diagnosis.Trim(),
                    Treatment = _treatment?.Trim() ?? string.Empty,
                    Notes = _notes?.Trim() ?? string.Empty,
                    Priority = _selectedPriority
                };

                var result = await _consultationService.RegisterAsync(
                    request, await SecureStorage.GetAsync(AppConstants.TokenKey));

                if (!result.Success)
                {
                    GeneralError = result.Error ?? "No se pudo registrar la consulta";
                    return;
                }

                if (_pendingImages.Count > 0)
                {
                    var token2 = await SecureStorage.GetAsync(AppConstants.TokenKey);
                    var serverId = result.Consultation?.Id;
                    var recent = (await LocalDatabase.Instance.GetConsultationsByPatientAsync(_patientId)).FirstOrDefault();
                    var localId = recent?.LocalId ?? 0;
                    _ = UploadPendingImagesAsync(localId, serverId, token2);
                }

                if (result.SavedOffline)
                {
                    ShowPendingSync = true;
                    SnackbarMessage = "Consulta guardada localmente. Pendiente de sincronización.";
                }
                else
                {
                    SnackbarMessage = "Consulta registrada exitosamente.";
                }

                IsSnackbarVisible = true;

                Device.StartTimer(TimeSpan.FromSeconds(2), () =>
                {
                    Device.BeginInvokeOnMainThread(async () => await Shell.Current.GoToAsync(".."));
                    return false;
                });
            }
            catch (Exception)
            {
                GeneralError = "Error al guardar la consulta";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private class PendingImage
        {
            public byte[] Bytes { get; set; }
            public string FileName { get; set; }
        }
    }

    public class PendingImageItem
    {
        public string FileName { get; set; }
        public ImageSource Source { get; set; }
    }
}
