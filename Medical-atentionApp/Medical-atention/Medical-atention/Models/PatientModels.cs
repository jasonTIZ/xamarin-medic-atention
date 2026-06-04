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
}
