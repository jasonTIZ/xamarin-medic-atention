using Medical_atention.Helpers;
using System;

namespace Medical_atention.Models
{
    public class Patient
    {
        public int Id { get; set; }
        public Guid LocalId { get; set; } = Guid.NewGuid();
        public bool PendingSync { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public int Priority { get; set; } = (int)PriorityLevel.Medium;
        public DateTime? LastConsultationAt { get; set; }
        public bool PendingPrioritySync { get; set; }
        public int? PendingPriority { get; set; }

        public string FullName => $"{FirstName} {LastName}".Trim();

        public PriorityLevel EffectivePriority =>
            PendingPrioritySync && PendingPriority.HasValue
                ? (PriorityLevel)PendingPriority.Value
                : (PriorityLevel)Priority;

        public string PriorityLabel => PriorityHelper.GetDisplayName(EffectivePriority);
    }
}
