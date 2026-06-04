using Medical_atention.Helpers;
using SQLite;

namespace Medical_atention.Models
{
    // Modelo de dominio y entidad SQLite.
    // El mapeo hacia/desde PatientResponseDto es responsabilidad de PatientService.
    [Table("patients")]
    public class Patient
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;

        [Column("PriorityLevel")]
        public int Priority { get; set; } = (int)PriorityLevel.Medium;

        public System.DateTime? LastConsultationAt { get; set; }

        // Soporte offline: cambio de prioridad pendiente de sincronizar.
        public bool PendingPrioritySync { get; set; }
        public int? PendingPriority { get; set; }

        // ── Propiedades calculadas para la Vista ──────────────────────────────
        [Ignore]
        public string FullName => $"{FirstName} {LastName}".Trim();

        [Ignore]
        public PriorityLevel EffectivePriority =>
            PendingPrioritySync && PendingPriority.HasValue
                ? (PriorityLevel)PendingPriority.Value
                : (PriorityLevel)Priority;

        [Ignore]
        public string PriorityLabel => PriorityHelper.GetDisplayName(EffectivePriority);
    }
}
