using System;
using SQLite;

namespace Medical_atention.Models
{
    public class PatientRequestDto
    {
        public string Name { get; set; }
        public string LastName { get; set; }
        public string IdentificationNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
    }

    public class PatientResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string IdentificationNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public PriorityLevel Priority { get; set; }
        public DateTime? LastConsultationAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public string FullName => $"{Name} {LastName}".Trim();
    }

    public class UpdatePatientPriorityRequest
    {
        public PriorityLevel Priority { get; set; }
    }

    [Table("patients")]
    public class PatientEntity
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string IdentificationNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public int Priority { get; set; }
        public DateTime? LastConsultationAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool PendingPrioritySync { get; set; }
        public int? PendingPriority { get; set; }

        public static PatientEntity FromDto(PatientResponseDto dto) =>
            new PatientEntity
            {
                Id = dto.Id,
                Name = dto.Name,
                LastName = dto.LastName,
                IdentificationNumber = dto.IdentificationNumber,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                Priority = (int)dto.Priority,
                LastConsultationAt = dto.LastConsultationAt,
                CreatedAt = dto.CreatedAt,
                PendingPrioritySync = false,
                PendingPriority = null
            };

        public PatientResponseDto ToDto() =>
            new PatientResponseDto
            {
                Id = Id,
                Name = Name,
                LastName = LastName,
                IdentificationNumber = IdentificationNumber,
                DateOfBirth = DateOfBirth,
                Gender = Gender,
                Priority = PendingPrioritySync && PendingPriority.HasValue
                    ? (PriorityLevel)PendingPriority.Value
                    : (PriorityLevel)Priority,
                LastConsultationAt = LastConsultationAt,
                CreatedAt = CreatedAt
            };
    }
}
