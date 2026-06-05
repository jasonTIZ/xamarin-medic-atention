using Medical_atention.Constants;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace Medical_atention.Models
{
    public class AttachmentItem : INotifyPropertyChanged
    {
        public int LocalId { get; set; }
        public int? ServerId { get; set; }
        public int? ConsultationServerId { get; set; }
        public string LocalPath { get; set; } = string.Empty;
        public string RemoteUrl { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public bool PendingSync { get; set; }

        public ImageSource ThumbnailSource
        {
            get
            {
                if (!string.IsNullOrEmpty(LocalPath))
                    return ImageSource.FromFile(LocalPath);
                if (!string.IsNullOrEmpty(RemoteUrl))
                    return ImageSource.FromUri(new Uri(RemoteUrl));
                return null;
            }
        }

        private bool _isUploading;
        public bool IsUploading
        {
            get => _isUploading;
            set { _isUploading = value; OnPropertyChanged(); OnPropertyChanged(nameof(ShowProgress)); }
        }

        private double _uploadProgress;
        public double UploadProgress
        {
            get => _uploadProgress;
            set { _uploadProgress = value; OnPropertyChanged(); OnPropertyChanged(nameof(UploadProgressText)); }
        }

        public bool ShowProgress => _isUploading;
        public string UploadProgressText => $"{(int)(_uploadProgress * 100)}%";

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string p = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }
}
