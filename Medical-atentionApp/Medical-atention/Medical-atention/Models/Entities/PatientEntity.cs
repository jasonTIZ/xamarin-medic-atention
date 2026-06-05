using System;
using SQLite;

namespace Medical_atention.Models.Entities
{
    [Table("patients")]
    public class PatientEntity
    {
        [PrimaryKey]
        public int Id { get; set; }

        public Guid LocalId { get; set; } = Guid.NewGuid();

        [Column("pending_sync")]
        public bool PendingSync { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;

        [Column("PriorityLevel")]
        public int Priority { get; set; } = (int)PriorityLevel.Medium;

        public DateTime? LastConsultationAt { get; set; }

        public bool PendingPrioritySync { get; set; }
        public int? PendingPriority { get; set; }
    }
}
