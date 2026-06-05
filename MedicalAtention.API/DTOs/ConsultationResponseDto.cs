namespace MedicalAtention.API.DTOs;

public record ConsultationResponseDto(
    int Id,
    int PatientId,
    DateTime ConsultationDate,
    string Symptoms,
    string Diagnosis,
    string Treatment,
    string Notes,
    DateTime CreatedAt);
