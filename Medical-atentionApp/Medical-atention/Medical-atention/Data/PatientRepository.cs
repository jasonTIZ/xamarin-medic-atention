using Medical_atention.Helpers;
using Medical_atention.Models;
using Medical_atention.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Medical_atention.Data
{
    public class PatientRepository : IPatientRepository
    {
        public async Task<List<PatientEntity>> GetAllAsync()
        {
            await LocalDatabase.InitializeAsync();
            var list = await LocalDatabase.Connection.Table<PatientEntity>().ToListAsync();
            return list
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .ToList();
        }

        public async Task<List<PatientEntity>> GetAllSortedByPriorityAsync()
        {
            await LocalDatabase.InitializeAsync();
            var list = await LocalDatabase.Connection.Table<PatientEntity>().ToListAsync();
            return list
                .OrderBy(p => PatientMapper.ToDomain(p).EffectivePriority)
                .ThenBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .ToList();
        }

        public async Task ReplaceAllAsync(IEnumerable<PatientEntity> patients)
        {
            await LocalDatabase.InitializeAsync();
            var pending = await LocalDatabase.Connection
                .Table<PatientEntity>()
                .Where(p => p.PendingPrioritySync || p.PendingSync)
                .ToListAsync();
            var pendingById = pending.Where(p => p.Id > 0).ToDictionary(p => p.Id);
            var pendingByLocalId = pending.Where(p => p.LocalId != Guid.Empty).ToDictionary(p => p.LocalId);

            await LocalDatabase.Connection.RunInTransactionAsync(conn =>
            {
                conn.DeleteAll<PatientEntity>();
                foreach (var patient in patients)
                {
                    if (patient.Id > 0 && pendingById.TryGetValue(patient.Id, out var byId))
                    {
                        patient.PendingPrioritySync = byId.PendingPrioritySync;
                        patient.PendingPriority = byId.PendingPriority;
                        patient.PendingSync = byId.PendingSync;
                        patient.LocalId = byId.LocalId;
                    }
                    conn.Insert(patient);
                }

                foreach (var localOnly in pending.Where(p => p.Id <= 0))
                {
                    if (!pendingByLocalId.ContainsKey(localOnly.LocalId)) continue;
                    conn.Insert(localOnly);
                }
            });
        }

        public async Task UpsertAsync(PatientEntity patient)
        {
            await LocalDatabase.InitializeAsync();
            PatientEntity existing = null;
            if (patient.Id > 0)
                existing = await LocalDatabase.Connection.FindAsync<PatientEntity>(patient.Id);
            if (existing == null && patient.LocalId != Guid.Empty)
                existing = await GetByLocalIdAsync(patient.LocalId);

            if (existing == null)
            {
                if (patient.LocalId == Guid.Empty)
                    patient.LocalId = Guid.NewGuid();
                await LocalDatabase.Connection.InsertAsync(patient);
                return;
            }

            if (patient.Id > 0 && existing.Id != patient.Id)
            {
                await LocalDatabase.Connection.ExecuteAsync(
                    "DELETE FROM patients WHERE LocalId = ?", existing.LocalId);
                patient.LocalId = existing.LocalId;
                await LocalDatabase.Connection.InsertAsync(patient);
                return;
            }

            patient.Id = existing.Id;
            patient.LocalId = existing.LocalId;
            patient.PendingPrioritySync = patient.PendingPrioritySync || existing.PendingPrioritySync;
            patient.PendingPriority = patient.PendingPriority ?? existing.PendingPriority;
            patient.PendingSync = patient.PendingSync || existing.PendingSync;
            await LocalDatabase.Connection.UpdateAsync(patient);
        }

        public async Task<PatientEntity> GetByIdAsync(int id)
        {
            await LocalDatabase.InitializeAsync();
            return await LocalDatabase.Connection
                .Table<PatientEntity>()
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<PatientEntity> GetByLocalIdAsync(Guid localId)
        {
            await LocalDatabase.InitializeAsync();
            return await LocalDatabase.Connection
                .Table<PatientEntity>()
                .Where(p => p.LocalId == localId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<PatientEntity>> GetPendingSyncAsync()
        {
            await LocalDatabase.InitializeAsync();
            return await LocalDatabase.Connection
                .Table<PatientEntity>()
                .Where(p => p.PendingSync)
                .ToListAsync();
        }

        public async Task DeleteAsync(int id)
        {
            await LocalDatabase.InitializeAsync();
            await LocalDatabase.Connection.DeleteAsync<PatientEntity>(id);
        }

        public async Task SetPendingPriorityAsync(int id, PriorityLevel priority)
        {
            await LocalDatabase.InitializeAsync();
            var patient = await LocalDatabase.Connection.FindAsync<PatientEntity>(id);
            if (patient == null) return;

            patient.PendingPrioritySync = true;
            patient.PendingPriority = (int)priority;
            patient.PendingSync = true;
            await LocalDatabase.Connection.UpdateAsync(patient);
        }

        public async Task ClearPendingPriorityAsync(int id, PriorityLevel syncedPriority)
        {
            await LocalDatabase.InitializeAsync();
            var patient = await LocalDatabase.Connection.FindAsync<PatientEntity>(id);
            if (patient == null) return;

            patient.Priority = (int)syncedPriority;
            patient.PendingPrioritySync = false;
            patient.PendingPriority = null;
            patient.PendingSync = false;
            await LocalDatabase.Connection.UpdateAsync(patient);
        }

        public async Task<List<PatientEntity>> GetPendingPriorityUpdatesAsync()
        {
            await LocalDatabase.InitializeAsync();
            return await LocalDatabase.Connection
                .Table<PatientEntity>()
                .Where(p => p.PendingPrioritySync)
                .ToListAsync();
        }
    }
}
