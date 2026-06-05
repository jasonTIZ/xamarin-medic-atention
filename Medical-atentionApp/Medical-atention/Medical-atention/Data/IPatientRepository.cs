using Medical_atention.Models;
using Medical_atention.Models.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Medical_atention.Data
{
    public interface IPatientRepository
    {
        Task<List<PatientEntity>> GetAllAsync();
        Task<List<PatientEntity>> GetAllSortedByPriorityAsync();
        Task<PatientEntity> GetByIdAsync(int id);
        Task<PatientEntity> GetByLocalIdAsync(Guid localId);
        Task<List<PatientEntity>> GetPendingSyncAsync();
        Task ReplaceAllAsync(IEnumerable<PatientEntity> patients);
        Task UpsertAsync(PatientEntity patient);
        Task DeleteAsync(int id);
        Task SetPendingPriorityAsync(int id, PriorityLevel priority);
        Task ClearPendingPriorityAsync(int id, PriorityLevel syncedPriority);
        Task<List<PatientEntity>> GetPendingPriorityUpdatesAsync();
    }
}
