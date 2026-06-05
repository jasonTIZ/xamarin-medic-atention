using Medical_atention.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Medical_atention.Data
{
    public class ConsultationRepository : IConsultationRepository
    {
        public async Task<List<ConsultationEntity>> GetAllAsync()
        {
            await LocalDatabase.InitializeAsync();
            return await LocalDatabase.Connection.Table<ConsultationEntity>().ToListAsync();
        }

        public async Task<ConsultationEntity> GetByIdAsync(int id)
        {
            await LocalDatabase.InitializeAsync();
            return await LocalDatabase.Connection.FindAsync<ConsultationEntity>(id);
        }

        public async Task<ConsultationEntity> GetByLocalIdAsync(Guid localId)
        {
            await LocalDatabase.InitializeAsync();
            return await LocalDatabase.Connection
                .Table<ConsultationEntity>()
                .Where(c => c.LocalId == localId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<ConsultationEntity>> GetPendingSyncAsync()
        {
            await LocalDatabase.InitializeAsync();
            return await LocalDatabase.Connection
                .Table<ConsultationEntity>()
                .Where(c => c.PendingSync)
                .ToListAsync();
        }

        public async Task<List<ConsultationEntity>> GetByPatientIdAsync(int patientId)
        {
            await LocalDatabase.InitializeAsync();
            return await LocalDatabase.Connection
                .Table<ConsultationEntity>()
                .Where(c => c.PatientId == patientId)
                .OrderByDescending(c => c.ConsultationDate)
                .ToListAsync();
        }

        public async Task InsertAsync(ConsultationEntity consultation)
        {
            await LocalDatabase.InitializeAsync();
            if (consultation.LocalId == Guid.Empty)
                consultation.LocalId = Guid.NewGuid();
            await LocalDatabase.Connection.InsertAsync(consultation);
        }

        public async Task UpdateAsync(ConsultationEntity consultation)
        {
            await LocalDatabase.InitializeAsync();
            await LocalDatabase.Connection.UpdateAsync(consultation);
        }

        public async Task DeleteAsync(int id)
        {
            await LocalDatabase.InitializeAsync();
            await LocalDatabase.Connection.DeleteAsync<ConsultationEntity>(id);
        }
    }
}
