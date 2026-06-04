using Medical_atention.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Medical_atention.Data
{
    public class PatientRepository
    {
        public async Task<List<PatientResponseDto>> GetAllSortedByPriorityAsync()
        {
            var db = await LocalDatabase.Instance.GetConnectionAsync();
            var entities = await db.Table<PatientEntity>().ToListAsync();
            return entities
                .Select(e => e.ToDto())
                .OrderBy(p => (int)p.Priority)
                .ThenBy(p => p.LastName)
                .ThenBy(p => p.Name)
                .ToList();
        }

        public async Task ReplaceAllAsync(IEnumerable<PatientResponseDto> patients)
        {
            var db = await LocalDatabase.Instance.GetConnectionAsync();
            var pending = await db.Table<PatientEntity>().Where(e => e.PendingPrioritySync).ToListAsync();
            var pendingById = pending.ToDictionary(e => e.Id);

            await db.RunInTransactionAsync(conn =>
            {
                conn.DeleteAll<PatientEntity>();
                foreach (var dto in patients)
                {
                    var entity = PatientEntity.FromDto(dto);
                    if (pendingById.TryGetValue(dto.Id, out var p))
                    {
                        entity.PendingPrioritySync = true;
                        entity.PendingPriority = p.PendingPriority;
                        entity.Priority = p.Priority;
                    }
                    conn.Insert(entity);
                }
            });
        }

        public async Task UpsertAsync(PatientResponseDto dto)
        {
            var db = await LocalDatabase.Instance.GetConnectionAsync();
            var entity = PatientEntity.FromDto(dto);
            var existing = await db.FindAsync<PatientEntity>(dto.Id);
            if (existing == null)
                await db.InsertAsync(entity);
            else
            {
                entity.PendingPrioritySync = existing.PendingPrioritySync;
                entity.PendingPriority = existing.PendingPriority;
                await db.UpdateAsync(entity);
            }
        }

        public async Task SetPendingPriorityAsync(int id, PriorityLevel priority)
        {
            var db = await LocalDatabase.Instance.GetConnectionAsync();
            var entity = await db.FindAsync<PatientEntity>(id);
            if (entity == null) return;

            entity.PendingPrioritySync = true;
            entity.PendingPriority = (int)priority;
            await db.UpdateAsync(entity);
        }

        public async Task ClearPendingPriorityAsync(int id, PriorityLevel syncedPriority)
        {
            var db = await LocalDatabase.Instance.GetConnectionAsync();
            var entity = await db.FindAsync<PatientEntity>(id);
            if (entity == null) return;

            entity.Priority = (int)syncedPriority;
            entity.PendingPrioritySync = false;
            entity.PendingPriority = null;
            await db.UpdateAsync(entity);
        }

        public async Task<List<PatientEntity>> GetPendingPriorityUpdatesAsync()
        {
            var db = await LocalDatabase.Instance.GetConnectionAsync();
            return await db.Table<PatientEntity>()
                .Where(e => e.PendingPrioritySync)
                .ToListAsync();
        }
    }
}
