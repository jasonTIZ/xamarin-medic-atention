using Medical_atention.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Medical_atention.Data
{
    public class SyncQueueRepository : ISyncQueueRepository
    {
        public async Task EnqueueAsync(SyncQueueEntity item)
        {
            await LocalDatabase.InitializeAsync();
            if (item.LocalId == Guid.Empty)
                item.LocalId = Guid.NewGuid();
            if (item.CreatedAt == default)
                item.CreatedAt = DateTime.UtcNow;
            await LocalDatabase.Connection.InsertAsync(item);
        }

        public async Task<List<SyncQueueEntity>> GetPendingOrderedAsync()
        {
            await LocalDatabase.InitializeAsync();
            var items = await LocalDatabase.Connection.Table<SyncQueueEntity>().ToListAsync();
            return items.OrderBy(i => i.CreatedAt).ThenBy(i => i.Id).ToList();
        }

        public async Task RemoveAsync(int id)
        {
            await LocalDatabase.InitializeAsync();
            await LocalDatabase.Connection.DeleteAsync<SyncQueueEntity>(id);
        }

        public async Task RemoveByEntityLocalIdAsync(Guid entityLocalId, string operation)
        {
            await LocalDatabase.InitializeAsync();
            var items = await LocalDatabase.Connection
                .Table<SyncQueueEntity>()
                .Where(q => q.EntityLocalId == entityLocalId && q.Operation == operation)
                .ToListAsync();

            foreach (var item in items)
                await LocalDatabase.Connection.DeleteAsync(item);
        }
    }
}
