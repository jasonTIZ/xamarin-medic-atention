namespace MedicalAtention.API.Models;

public class Consultation
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public DateTime ConsultationDate { get; set; }
    public string Symptoms { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public string Treatment { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string Priority { get; set; } = "medium";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsSynced { get; set; } = true;
}
