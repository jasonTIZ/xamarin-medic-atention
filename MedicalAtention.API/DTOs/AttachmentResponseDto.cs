namespace MedicalAtention.API.DTOs;

public record AttachmentResponseDto(
    int Id,
    int ConsultationId,
    string FileName,
    string Url,
    long FileSizeBytes,
    DateTime UploadedAt);
