using System;
using System.Globalization;

namespace Medical_atention.Models
{
    public class ConsultationListItem
    {
        private static readonly CultureInfo EsCulture = CreateSpanishCulture();

        private static CultureInfo CreateSpanishCulture()
        {
            try { return new CultureInfo("es-ES"); }
            catch { return CultureInfo.CurrentCulture; }
        }

        public int? ServerId { get; set; }
        public int LocalId { get; set; }
        public int PatientId { get; set; }
        public DateTime ConsultationDate { get; set; }
        public string Symptoms { get; set; }
        public string Diagnosis { get; set; }
        public string Treatment { get; set; }
        public string Notes { get; set; }
        public string Priority { get; set; }
        public bool PendingSync { get; set; }

        public string DateDisplay =>
            ConsultationDate.ToString("dd MMM, yyyy - HH:mm", EsCulture);

        public string DiagnosisTitle => Truncate(Diagnosis, 48);

        public string SummaryPreview
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Treatment))
                    return Truncate(Treatment, 72);
                if (!string.IsNullOrWhiteSpace(Notes))
                    return Truncate(Notes, 72);
                return Truncate(Symptoms, 72);
            }
        }

        private static string Truncate(string value, int max)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            value = value.Trim();
            return value.Length <= max ? value : value.Substring(0, max) + "...";
        }
    }
}
