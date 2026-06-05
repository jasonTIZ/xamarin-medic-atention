using Medical_atention.Models.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Medical_atention.Data
{
    public interface IConsultationRepository
    {
        Task<List<ConsultationEntity>> GetAllAsync();
        Task<ConsultationEntity> GetByIdAsync(int id);
        Task<ConsultationEntity> GetByLocalIdAsync(Guid localId);
        Task<List<ConsultationEntity>> GetPendingSyncAsync();
        Task<List<ConsultationEntity>> GetByPatientIdAsync(int patientId);
        Task InsertAsync(ConsultationEntity consultation);
        Task UpdateAsync(ConsultationEntity consultation);
        Task DeleteAsync(int id);
    }
}
