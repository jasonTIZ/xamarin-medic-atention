using Medical_atention.Models;
using System;
using System.Collections.Generic;
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
        Task<List<ConsultationListItem>> GetByPatientAsync(int patientId, DateTime from, DateTime to, string token);
        Task<ConsultationResponseDto> GetDetailAsync(int? serverId, int localId, string token);
        Task<(bool success, string error)> UpdateAsync(int? serverId, int localId, ConsultationUpdateDto request, string token);
    }
}
