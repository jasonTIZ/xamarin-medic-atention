using Medical_atention.Models.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Medical_atention.Data
{
    public interface ISyncQueueRepository
    {
        Task EnqueueAsync(SyncQueueEntity item);
        Task<List<SyncQueueEntity>> GetPendingOrderedAsync();
        Task RemoveAsync(int id);
        Task RemoveByEntityLocalIdAsync(Guid entityLocalId, string operation);
    }
}
