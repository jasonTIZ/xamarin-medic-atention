using Medical_atention.Models;
using Medical_atention.Models.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Medical_atention.Data
{
    public class ReminderRepository : IReminderRepository
    {
        public async Task<int> AddAsync(ReminderEntity reminder)
        {
            await LocalDatabase.InitializeAsync();
            if (reminder.CreatedAt == default)
                reminder.CreatedAt = DateTime.UtcNow;
            await LocalDatabase.Connection.InsertAsync(reminder);
            // sqlite-net rellena la PK autoincremental en la instancia tras el insert.
            return reminder.Id;
        }

        public async Task<List<ReminderEntity>> GetScheduledByPatientAsync(int patientId)
        {
            await LocalDatabase.InitializeAsync();
            return await LocalDatabase.Connection
                .Table<ReminderEntity>()
                .Where(r => r.PatientId == patientId && r.Status == ReminderStatus.Scheduled)
                .ToListAsync();
        }

        public async Task MarkCancelledAsync(int id)
        {
            await LocalDatabase.InitializeAsync();
            var reminder = await LocalDatabase.Connection.FindAsync<ReminderEntity>(id);
            if (reminder == null) return;

            reminder.Status = ReminderStatus.Cancelled;
            await LocalDatabase.Connection.UpdateAsync(reminder);
        }
    }
}
