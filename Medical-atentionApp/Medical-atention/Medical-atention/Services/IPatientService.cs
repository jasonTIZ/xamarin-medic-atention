using Medical_atention.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Medical_atention.Services
{
    public interface IPatientService
    {
        bool IsOnline();

        // ── Listado y detalle ──────────────────────────────────────────────────
        Task<IReadOnlyList<Patient>> LoadPatientsAsync(bool forceRefresh = false);
        Task<IReadOnlyList<Patient>> LoadPatientsFromLocalAsync();
        Task<Patient> GetPatientAsync(int id);

        // ── Registro ──────────────────────────────────────────────────────────
        // Recibe el DTO del formulario, devuelve el modelo de dominio o un error.
        Task<(Patient patient, string error)> RegisterAsync(PatientRequestDto request);

        // ── Triaje ────────────────────────────────────────────────────────────
        Task<(IReadOnlyList<Patient> patients, bool fromCache, string error)> GetPatientsByPriorityAsync();
        Task<(bool success, string error)> UpdatePriorityAsync(int id, PriorityLevel priority);
        Task SyncPendingPriorityChangesAsync();
    }
}
