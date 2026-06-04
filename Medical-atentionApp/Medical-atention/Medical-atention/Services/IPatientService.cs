using Medical_atention.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Medical_atention.Services
{
    public interface IPatientService
    {
        Task<IReadOnlyList<Patient>> LoadPatientsAsync(bool forceRefresh = false);
        Task<Patient> GetPatientAsync(int id);
        bool IsOnline();
    }
}
