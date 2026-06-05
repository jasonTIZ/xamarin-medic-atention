using SQLite;
using System;

namespace Medical_atention.Data
{
    [Table("consultations")]
    public class LocalConsultation
    {
        [PrimaryKey, AutoIncrement]
        public int LocalId { get; set; }

        public int? ServerId { get; set; }
        public int PatientId { get; set; }
        public DateTime ConsultationDate { get; set; }
        public string Symptoms { get; set; }
        public string Diagnosis { get; set; }
        public string Treatment { get; set; }
        public string Notes { get; set; }
        public string Priority { get; set; }
        public bool PendingSync { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
