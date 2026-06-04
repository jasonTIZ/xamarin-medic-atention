using System;
using System.Collections.Generic;

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
        public DateTime CreatedAt { get; set; }

        public string FullName => $"{Name} {LastName}".Trim();

        public string Initials
        {
            get
            {
                var a = string.IsNullOrWhiteSpace(Name) ? '?' : char.ToUpper(Name.Trim()[0]);
                var b = string.IsNullOrWhiteSpace(LastName) ? '?' : char.ToUpper(LastName.Trim()[0]);
                return $"{a}{b}";
            }
        }
    }
}
