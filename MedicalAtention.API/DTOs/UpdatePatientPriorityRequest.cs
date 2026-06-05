using MedicalAtention.API.Models;

namespace MedicalAtention.API.DTOs;

public class UpdatePatientPriorityRequest
{
    public PriorityLevel Priority { get; set; }
}
