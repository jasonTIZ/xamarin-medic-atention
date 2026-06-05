namespace MedicalAtention.API.DTOs;

public class ConsultationRequestDto
{
    public int PatientId { get; set; }
    public DateTime ConsultationDate { get; set; } = DateTime.UtcNow;
    public string Symptoms { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public string Treatment { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
