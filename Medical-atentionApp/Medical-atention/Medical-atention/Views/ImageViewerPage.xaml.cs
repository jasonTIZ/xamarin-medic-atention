using Medical_atention.Constants;
using Medical_atention.Data;
using System;
using System.IO;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Medical_atention.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [QueryProperty(nameof(LocalId), "localId")]
    public partial class ImageViewerPage : ContentPage
    {
        private double _startScale = 1;
        private string _localPath;
        private string _remoteUrl;
        private string _fileName;

        public ImageViewerPage()
        {
            InitializeComponent();
        }

        public string LocalId
        {
            set
            {
                if (int.TryParse(value, out var id))
                    _ = LoadAttachmentAsync(id);
            }
        }

        private async System.Threading.Tasks.Task LoadAttachmentAsync(int localId)
        {
            try
            {
                await LocalDatabase.InitializeAsync();
                var entity = await LocalDatabase.Connection.Table<Models.Entities.AttachmentEntity>()
                    .FirstOrDefaultAsync(a => a.Id == localId);

                if (entity is null)
                {
                    ShowError();
                    return;
                }

                _localPath = entity.LocalPath;
                _remoteUrl = entity.RemoteUrl;
                _fileName = entity.FileName;
                FileNameLabel.Text = entity.FileName;

                ImageSource source = null;
                if (!string.IsNullOrEmpty(entity.LocalPath) && File.Exists(entity.LocalPath))
                    source = ImageSource.FromFile(entity.LocalPath);
                else if (!string.IsNullOrEmpty(entity.RemoteUrl))
                    source = ImageSource.FromUri(new Uri(entity.RemoteUrl));

                if (source is null) { ShowError(); return; }

                ViewerImage.Source = source;
                ViewerImage.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == nameof(Image.IsLoading) && !ViewerImage.IsLoading)
                        Device.BeginInvokeOnMainThread(() => LoadingIndicator.IsVisible = false);
                };
            }
            catch (Exception)
            {
                ShowError();
            }
        }

        private void ShowError()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                LoadingIndicator.IsVisible = false;
                ErrorView.IsVisible = true;
            });
        }

        private void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
        {
            if (e.Status == GestureStatus.Started)
                _startScale = ViewerImage.Scale;

            if (e.Status == GestureStatus.Running)
                ViewerImage.Scale = Math.Max(1, Math.Min(_startScale * e.Scale, 5));

            if (e.Status == GestureStatus.Completed && ViewerImage.Scale < 1)
                ViewerImage.Scale = 1;
        }

        private void OnDoubleTap(object sender, EventArgs e)
        {
            ViewerImage.ScaleTo(ViewerImage.Scale > 1 ? 1 : 2.5, 250, Easing.CubicInOut);
        }

        private void OnBackClicked(object sender, EventArgs e)
        {
            Shell.Current.GoToAsync("..");
        }

        private async void OnDownloadClicked(object sender, EventArgs e)
        {
            try
            {
                string path = _localPath;

                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                {
                    if (string.IsNullOrEmpty(_remoteUrl)) return;
                    var bytes = await new System.Net.Http.HttpClient().GetByteArrayAsync(_remoteUrl);
                    path = Path.Combine(Path.GetTempPath(), _fileName ?? $"{Guid.NewGuid()}.jpg");
                    using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                        await fs.WriteAsync(bytes, 0, bytes.Length);
                }

                await Share.RequestAsync(new ShareFileRequest
                {
                    Title = _fileName ?? "Imagen",
                    File = new ShareFile(path)
                });
            }
            catch (Exception)
            {
                await DisplayAlert("Error", "No se pudo guardar la imagen", "OK");
            }
        }
    }
}
