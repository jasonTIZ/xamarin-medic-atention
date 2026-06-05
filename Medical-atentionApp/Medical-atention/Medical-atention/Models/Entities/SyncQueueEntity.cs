using System;
using SQLite;

namespace Medical_atention.Models.Entities
{
    [Table("sync_queue")]
    public class SyncQueueEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public Guid LocalId { get; set; } = Guid.NewGuid();
        public string EntityType { get; set; } = string.Empty;
        public Guid EntityLocalId { get; set; }
        public string Operation { get; set; } = string.Empty;
        public string PayloadJson { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
