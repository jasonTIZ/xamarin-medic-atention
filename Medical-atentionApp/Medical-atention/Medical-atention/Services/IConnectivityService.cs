namespace Medical_atention.Services
{
    public interface IConnectivityService
    {
        bool IsConnected { get; }
        void Start();
        void Stop();
    }
}
