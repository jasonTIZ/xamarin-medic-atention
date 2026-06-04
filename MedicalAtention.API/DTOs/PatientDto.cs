namespace MedicalAtention.API.DTOs;

public record PatientDto(
    int Id,
    string FirstName,
    string LastName,
    string FullName,
    string? DocumentNumber,
    int PriorityLevel);
