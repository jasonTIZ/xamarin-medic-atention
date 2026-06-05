using System;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Medical_atention.Services
{
    public class ConnectivityService : IConnectivityService
    {
        public static ConnectivityService Instance { get; } = new ConnectivityService();

        private readonly ISyncService _syncService;
        private bool _started;

        public ConnectivityService() : this(new SyncService()) { }

        public ConnectivityService(ISyncService syncService)
        {
            _syncService = syncService;
        }

        public bool IsConnected => Connectivity.NetworkAccess == NetworkAccess.Internet;

        public void Start()
        {
            if (_started) return;
            Connectivity.ConnectivityChanged += OnConnectivityChanged;
            _started = true;
        }

        public void Stop()
        {
            if (!_started) return;
            Connectivity.ConnectivityChanged -= OnConnectivityChanged;
            _started = false;
        }

        private async void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            if (e.NetworkAccess != NetworkAccess.Internet) return;
            try
            {
                await _syncService.SyncPendingAsync();
            }
            catch (Exception)
            {
                // La sincronización se reintentará en la próxima reconexión o carga de pantalla.
            }
        }
    }
}
