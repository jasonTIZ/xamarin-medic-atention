using System;
using Medical_atention.Models;
using SQLite;

namespace Medical_atention.Models.Entities
{
    // Recordatorio de seguimiento programado en el dispositivo.
    // El Id autoincremental se reutiliza como NotificationId del SO,
    // garantizando un identificador único y estable para poder cancelarlo.
    [Table("reminders")]
    public class ReminderEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int PatientId { get; set; }

        public string PatientName { get; set; } = string.Empty;

        public string Reason { get; set; } = string.Empty;

        public DateTime NotifyTime { get; set; }

        [Column("status")]
        public string Status { get; set; } = ReminderStatus.Scheduled;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
