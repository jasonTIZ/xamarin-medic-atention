using System;

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

    public class ConsultationRequestDto
    {
        public int PatientId { get; set; }
        public DateTime ConsultationDate { get; set; } = DateTime.UtcNow;
        public string Symptoms { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public string Treatment { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class ConsultationResponseDto
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public DateTime ConsultationDate { get; set; }
        public string Symptoms { get; set; }
        public string Diagnosis { get; set; }
        public string Treatment { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
