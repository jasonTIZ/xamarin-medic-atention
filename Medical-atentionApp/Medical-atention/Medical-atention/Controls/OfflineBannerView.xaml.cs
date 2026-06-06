using Medical_atention.Constants;
using Medical_atention.Helpers;
using Medical_atention.Services;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Medical_atention.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OfflineBannerView : ContentView
    {
        private const uint FadeInMs = 250;
        private const uint FadeOutMs = 200;
        private bool _bannerShown;
        private bool _subscribed;
        private bool _animating;

        public OfflineBannerView()
        {
            InitializeComponent();
        }

        protected override void OnParentSet()
        {
            base.OnParentSet();

            if (Parent != null)
            {
                Subscribe();
                _ = RefreshStateAsync(animate: false);
            }
            else
            {
                Unsubscribe();
            }
        }

        public void Refresh()
        {
            _ = RefreshStateAsync(animate: false);
        }

        private void Subscribe()
        {
            if (_subscribed) return;
            Connectivity.ConnectivityChanged += OnConnectivityChanged;
            MessagingCenter.Subscribe<object, int>(
                this, SyncNotificationHelper.SyncCompletedMessage, OnSyncCompleted);
            _subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_subscribed) return;
            Connectivity.ConnectivityChanged -= OnConnectivityChanged;
            MessagingCenter.Unsubscribe<object, int>(this, SyncNotificationHelper.SyncCompletedMessage);
            _subscribed = false;
        }

        private void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() => _ = RefreshStateAsync(animate: true));
        }

        private void OnSyncCompleted(object sender, int syncedCount)
        {
            Device.BeginInvokeOnMainThread(() => _ = RefreshStateAsync(animate: false));
        }

        private async Task RefreshStateAsync(bool animate)
        {
            var isOffline = !ConnectivityService.Instance.IsConnected;

            if (!isOffline)
            {
                await SetBannerVisibleAsync(false, animate);
                return;
            }

            await UpdateSyncLabelAsync();
            await SetBannerVisibleAsync(true, animate);
        }

        private async Task UpdateSyncLabelAsync()
        {
            try
            {
                var raw = await SecureStorage.GetAsync(AppConstants.LastPatientSyncKey);
                if (!string.IsNullOrEmpty(raw) && DateTime.TryParse(raw, out var sync))
                {
                    SyncLabel.Text = $"Última sincronización: {sync.ToLocalTime():dd/MM/yyyy HH:mm}";
                    SyncLabel.IsVisible = true;
                    return;
                }
            }
            catch (Exception)
            {
                // SecureStorage no disponible en esta plataforma.
            }

            SyncLabel.IsVisible = false;
        }

        private async Task SetBannerVisibleAsync(bool show, bool animate)
        {
            if (_bannerShown == show || _animating)
                return;

            _animating = true;
            try
            {
                if (show)
                {
                    if (!_bannerShown)
                    {
                        IsVisible = true;
                        Opacity = 0;
                        if (animate)
                            await this.FadeTo(1, FadeInMs, Easing.CubicOut);
                        else
                            Opacity = 1;
                        _bannerShown = true;
                    }
                }
                else if (_bannerShown)
                {
                    if (animate)
                        await this.FadeTo(0, FadeOutMs, Easing.CubicIn);
                    else
                        Opacity = 0;
                    IsVisible = false;
                    _bannerShown = false;
                }
            }
            finally
            {
                _animating = false;
            }
        }
    }
}
