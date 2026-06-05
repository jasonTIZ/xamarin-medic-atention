using Medical_atention.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Medical_atention.Services
{
    public interface IPatientService
    {
        // Detalle / CRUD (dev)
        Task<(PatientResponseDto patient, string error)> RegisterAsync(PatientRequestDto request, string token);
        Task<PatientResponseDto> GetPatientAsync(int id, string token);
        Task<List<PatientResponseDto>> GetAllPatientsAsync(string token);
        Task<(PatientResponseDto patient, string error)> UpdateAsync(int id, PatientRequestDto request, string token);
        Task<bool> DeleteAsync(int id, string token);
        Task<PatientHistorySummary> GetPatientHistoryAsync(int patientId, string token);

        // Triaje / offline
        bool IsOnline();
        Task<IReadOnlyList<Patient>> LoadPatientsAsync(bool forceRefresh = false);
        Task<IReadOnlyList<Patient>> LoadPatientsFromLocalAsync();
        Task<(Patient patient, string error)> RegisterLocalAsync(PatientRequestDto request);
        Task<(IReadOnlyList<Patient> patients, bool fromCache, string error)> GetPatientsByPriorityAsync();
        Task<(bool success, string error)> UpdatePriorityAsync(int id, PriorityLevel priority);
        Task SyncPendingPriorityChangesAsync();
    }
}
