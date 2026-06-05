using System;
using SQLite;

namespace Medical_atention.Models.Entities
{
    [Table("consultations")]
    public class ConsultationEntity
    {
        [PrimaryKey]
        public int Id { get; set; }

        public Guid LocalId { get; set; } = Guid.NewGuid();

        [Column("pending_sync")]
        public bool PendingSync { get; set; }

        public int PatientId { get; set; }
        public Guid PatientLocalId { get; set; }
        public DateTime ConsultationDate { get; set; } = DateTime.UtcNow;
        public string Symptoms { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public string Treatment { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
