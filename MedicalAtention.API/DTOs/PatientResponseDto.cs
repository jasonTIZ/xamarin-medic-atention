namespace MedicalAtention.API.DTOs;

public record PatientResponseDto(int Id, string Name, string LastName, string IdentificationNumber, DateTime DateOfBirth, string Gender, DateTime CreatedAt);
