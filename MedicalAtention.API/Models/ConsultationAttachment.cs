namespace MedicalAtention.API.Models;

public class ConsultationAttachment
{
    public int Id { get; set; }
    public int ConsultationId { get; set; }
    public Consultation Consultation { get; set; } = null!;
    public string FileName { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
