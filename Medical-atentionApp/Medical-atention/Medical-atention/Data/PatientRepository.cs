using Medical_atention.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Medical_atention.Data
{
    public class PatientRepository
    {
        public async Task<List<Patient>> GetAllAsync()
        {
            await LocalDatabase.InitializeAsync();
            var list = await LocalDatabase.Connection.Table<Patient>().ToListAsync();
            return list
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .ToList();
        }

        public async Task<List<Patient>> GetAllSortedByPriorityAsync()
        {
            await LocalDatabase.InitializeAsync();
            var list = await LocalDatabase.Connection.Table<Patient>().ToListAsync();
            return list
                .OrderBy(p => p.EffectivePriority)
                .ThenBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .ToList();
        }

        public async Task ReplaceAllAsync(IEnumerable<Patient> patients)
        {
            await LocalDatabase.InitializeAsync();
            var pending = await LocalDatabase.Connection
                .Table<Patient>()
                .Where(p => p.PendingPrioritySync)
                .ToListAsync();
            var pendingById = pending.ToDictionary(p => p.Id);

            await LocalDatabase.Connection.RunInTransactionAsync(conn =>
            {
                conn.DeleteAll<Patient>();
                foreach (var patient in patients)
                {
                    if (pendingById.TryGetValue(patient.Id, out var p))
                    {
                        patient.PendingPrioritySync = true;
                        patient.PendingPriority = p.PendingPriority;
                        patient.Priority = p.Priority;
                    }
                    conn.Insert(patient);
                }
            });
        }

        public async Task UpsertAsync(Patient patient)
        {
            await LocalDatabase.InitializeAsync();
            var existing = await LocalDatabase.Connection.FindAsync<Patient>(patient.Id);
            if (existing == null)
                await LocalDatabase.Connection.InsertAsync(patient);
            else
            {
                patient.PendingPrioritySync = existing.PendingPrioritySync;
                patient.PendingPriority = existing.PendingPriority;
                await LocalDatabase.Connection.UpdateAsync(patient);
            }
        }

        public async Task<Patient> GetByIdAsync(int id)
        {
            await LocalDatabase.InitializeAsync();
            return await LocalDatabase.Connection
                .Table<Patient>()
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task SetPendingPriorityAsync(int id, PriorityLevel priority)
        {
            await LocalDatabase.InitializeAsync();
            var patient = await LocalDatabase.Connection.FindAsync<Patient>(id);
            if (patient == null) return;

            patient.PendingPrioritySync = true;
            patient.PendingPriority = (int)priority;
            await LocalDatabase.Connection.UpdateAsync(patient);
        }

        public async Task ClearPendingPriorityAsync(int id, PriorityLevel syncedPriority)
        {
            await LocalDatabase.InitializeAsync();
            var patient = await LocalDatabase.Connection.FindAsync<Patient>(id);
            if (patient == null) return;

            patient.Priority = (int)syncedPriority;
            patient.PendingPrioritySync = false;
            patient.PendingPriority = null;
            await LocalDatabase.Connection.UpdateAsync(patient);
        }

        public async Task<List<Patient>> GetPendingPriorityUpdatesAsync()
        {
            await LocalDatabase.InitializeAsync();
            return await LocalDatabase.Connection
                .Table<Patient>()
                .Where(p => p.PendingPrioritySync)
                .ToListAsync();
        }
    }
}
