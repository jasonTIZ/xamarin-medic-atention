using SQLite;
using System;

namespace Medical_atention.Models.Entities
{
    [Table("attachments")]
    public class AttachmentEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int? ServerId { get; set; }
        public int ConsultationLocalId { get; set; }
        public int? ConsultationServerId { get; set; }
        public string LocalPath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string RemoteUrl { get; set; } = string.Empty;

        [Column("pending_sync")]
        public bool PendingSync { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
