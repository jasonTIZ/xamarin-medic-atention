using Medical_atention.Models;
using System;
using Xamarin.Forms;

namespace Medical_atention.Helpers
{
    public static class PriorityHelper
    {
        public static string GetDisplayName(PriorityLevel level)
        {
            switch (level)
            {
                case PriorityLevel.Urgent: return "Urgente";
                case PriorityLevel.High: return "Alta";
                case PriorityLevel.Medium: return "Media";
                case PriorityLevel.Low: return "Baja";
                default: return level.ToString();
            }
        }

        public static Color GetBadgeColor(PriorityLevel level)
        {
            switch (level)
            {
                case PriorityLevel.Urgent: return Color.FromHex("#C53030");
                case PriorityLevel.High: return Color.FromHex("#DD6B20");
                case PriorityLevel.Medium: return Color.FromHex("#D69E2E");
                case PriorityLevel.Low: return Color.FromHex("#38A169");
                default: return Color.FromHex("#718096");
            }
        }

        public static Color GetGroupBackgroundColor(PriorityLevel level)
        {
            switch (level)
            {
                case PriorityLevel.Urgent: return Color.FromHex("#FED7D7");
                case PriorityLevel.High: return Color.FromHex("#FEEBC8");
                case PriorityLevel.Medium: return Color.FromHex("#FEFCBF");
                case PriorityLevel.Low: return Color.FromHex("#C6F6D5");
                default: return Color.FromHex("#EDF2F7");
            }
        }

        public static string FormatTimeSinceLastConsultation(DateTime? lastConsultation)
        {
            if (!lastConsultation.HasValue)
                return "Sin consultas previas";

            var span = DateTime.UtcNow - lastConsultation.Value.ToUniversalTime();
            if (span.TotalMinutes < 1) return "Última consulta: hace un momento";
            if (span.TotalHours < 1) return $"Última consulta: hace {(int)span.TotalMinutes} min";
            if (span.TotalDays < 1) return $"Última consulta: hace {(int)span.TotalHours} h";
            if (span.TotalDays < 30) return $"Última consulta: hace {(int)span.TotalDays} días";
            return $"Última consulta: {lastConsultation.Value:dd/MM/yyyy}";
        }

        public static PriorityLevel[] AllLevels { get; } =
        {
            PriorityLevel.Urgent,
            PriorityLevel.High,
            PriorityLevel.Medium,
            PriorityLevel.Low
        };
    }
}
