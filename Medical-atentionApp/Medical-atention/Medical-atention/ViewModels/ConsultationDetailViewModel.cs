using Medical_atention.Constants;
using Medical_atention.Models;
using Medical_atention.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
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
        private readonly IAttachmentService _attachmentService;
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

        private ObservableCollection<AttachmentItem> _attachments = new ObservableCollection<AttachmentItem>();
        private bool _isLoadingAttachments;
        private string _attachmentError = string.Empty;

        public ConsultationDetailViewModel() : this(new ConsultationService(), new AttachmentService()) { }

        public ConsultationDetailViewModel(IConsultationService consultationService, IAttachmentService attachmentService)
        {
            _consultationService = consultationService;
            _attachmentService = attachmentService;
            EditCommand = new Command(StartEdit, () => !IsLoading && !IsEditing);
            SaveCommand = new Command(async () => await SaveAsync(), () => !IsSaving);
            CancelCommand = new Command(CancelEdit, () => IsEditing);
            AddFromGalleryCommand = new Command(async () => await AddAttachmentAsync(fromCamera: false), () => CanAddAttachment);
            AddFromCameraCommand = new Command(async () => await AddAttachmentAsync(fromCamera: true), () => CanAddAttachment);
            DeleteAttachmentCommand = new Command<AttachmentItem>(async item => await DeleteAttachmentAsync(item));
            OpenViewerCommand = new Command<AttachmentItem>(OpenViewer);
            SaveAttachmentToGalleryCommand = new Command<AttachmentItem>(async item => await SaveAttachmentToGalleryAsync(item));
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

        public ObservableCollection<AttachmentItem> Attachments
        {
            get => _attachments;
            set { _attachments = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanAddAttachment)); OnPropertyChanged(nameof(AttachmentCountLabel)); }
        }

        public bool IsLoadingAttachments
        {
            get => _isLoadingAttachments;
            set { _isLoadingAttachments = value; OnPropertyChanged(); }
        }

        public string AttachmentError
        {
            get => _attachmentError;
            set { _attachmentError = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasAttachmentError)); }
        }

        public bool HasAttachmentError => !string.IsNullOrEmpty(_attachmentError);
        public bool CanAddAttachment => _attachments.Count < 3;
        public string AttachmentCountLabel => $"{_attachments.Count}/3 imágenes";
        public bool HasAttachments => _attachments.Count > 0;

        public ICommand EditCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand AddFromGalleryCommand { get; }
        public ICommand AddFromCameraCommand { get; }
        public ICommand DeleteAttachmentCommand { get; }
        public ICommand OpenViewerCommand { get; }
        public ICommand SaveAttachmentToGalleryCommand { get; }

        public async Task LoadAsync(int? serverId, int localId)
        {
            _serverId = serverId > 0 ? serverId : null;
            _localId = localId;

            IsLoading = true;
            ErrorMessage = string.Empty;
            try
            {
                var token = await SecureStorage.GetAsync(AppConstants.TokenKey);
                var detail = await _consultationService.GetDetailAsync(_serverId, _localId, token);

                if (detail is null)
                {
                    ErrorMessage = "No se pudo cargar la consulta";
                    return;
                }

                ApplyDetail(detail);
                _ = LoadAttachmentsAsync(token);
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

        private async Task LoadAttachmentsAsync(string token)
        {
            IsLoadingAttachments = true;
            AttachmentError = string.Empty;
            try
            {
                var list = await _attachmentService.LoadForConsultationAsync(_localId, _serverId, token);
                Device.BeginInvokeOnMainThread(() =>
                {
                    Attachments = new ObservableCollection<AttachmentItem>(list);
                    OnPropertyChanged(nameof(HasAttachments));
                    OnPropertyChanged(nameof(CanAddAttachment));
                    OnPropertyChanged(nameof(AttachmentCountLabel));
                    RefreshAttachmentCommands();
                });
            }
            catch (Exception)
            {
                AttachmentError = "No se pudieron cargar las imágenes";
            }
            finally
            {
                IsLoadingAttachments = false;
            }
        }

        private async Task AddAttachmentAsync(bool fromCamera)
        {
            if (!CanAddAttachment) return;
            AttachmentError = string.Empty;

            var token = await SecureStorage.GetAsync(AppConstants.TokenKey);
            var progress = new Progress<double>(p =>
            {
                var uploading = _attachments.FirstOrDefault(a => a.IsUploading);
                if (uploading != null) uploading.UploadProgress = p;
            });

            var (item, error) = fromCamera
                ? await _attachmentService.AddFromCameraAsync(_localId, _serverId, token, progress)
                : await _attachmentService.AddFromGalleryAsync(_localId, _serverId, token, progress);

            if (item is null)
            {
                if (!string.IsNullOrEmpty(error)) AttachmentError = error;
                return;
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                Attachments.Add(item);
                OnPropertyChanged(nameof(HasAttachments));
                OnPropertyChanged(nameof(CanAddAttachment));
                OnPropertyChanged(nameof(AttachmentCountLabel));
                RefreshAttachmentCommands();
            });
        }

        private async Task DeleteAttachmentAsync(AttachmentItem item)
        {
            if (item is null) return;
            var token = await SecureStorage.GetAsync(AppConstants.TokenKey);
            var error = await _attachmentService.DeleteAsync(item, token);

            if (!string.IsNullOrEmpty(error))
            {
                AttachmentError = error;
                return;
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                Attachments.Remove(item);
                OnPropertyChanged(nameof(HasAttachments));
                OnPropertyChanged(nameof(CanAddAttachment));
                OnPropertyChanged(nameof(AttachmentCountLabel));
                RefreshAttachmentCommands();
            });
        }

        private static void OpenViewer(AttachmentItem item)
        {
            if (item is null) return;
            Shell.Current.GoToAsync($"ImageViewerPage?localId={item.LocalId}");
        }

        private async Task SaveAttachmentToGalleryAsync(AttachmentItem item)
        {
            var error = await _attachmentService.SaveToGalleryAsync(item);
            if (!string.IsNullOrEmpty(error)) AttachmentError = error;
        }

        private void RefreshAttachmentCommands()
        {
            ((Command)AddFromGalleryCommand).ChangeCanExecute();
            ((Command)AddFromCameraCommand).ChangeCanExecute();
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
