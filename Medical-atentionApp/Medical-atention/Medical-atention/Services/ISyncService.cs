using System.Threading.Tasks;

namespace Medical_atention.Services
{
    public interface ISyncService
    {
        Task<int> SyncPendingAsync();
    }
}
