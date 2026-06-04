using Medical_atention.Models;
using System.Threading.Tasks;

namespace Medical_atention.Services
{
    public class RegisterConsultationResult
    {
        public bool Success { get; set; }
        public bool SavedOffline { get; set; }
        public string Error { get; set; }
        public ConsultationResponseDto Consultation { get; set; }
    }

    public interface IConsultationService
    {
        Task<RegisterConsultationResult> RegisterAsync(ConsultationRequestDto request, string token);
    }
}
