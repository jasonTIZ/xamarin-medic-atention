using System.Threading.Tasks;

namespace Medical_atention.Services
{
    public interface IConsultationService
    {
        Task<(bool success, string error)> RegisterConsultationAsync(int patientId);
    }
}
