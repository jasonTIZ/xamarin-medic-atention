using System.Collections.Generic;

namespace Medical_atention.Models
{
    public class DiagnosisSummary
    {
        public string Diagnosis { get; set; }
        public string Date { get; set; }
    }

    public class PriorityPoint
    {
        public string Date { get; set; }
        public string Priority { get; set; }
    }

    public class PatientHistorySummary
    {
        public int PatientId { get; set; }
        public string FullName { get; set; }
        public int TotalConsultations { get; set; }
        public string LastConsultationDate { get; set; }
        public string CurrentPriority { get; set; }
        public List<DiagnosisSummary> RecentDiagnoses { get; set; } = new List<DiagnosisSummary>();
        public List<string> FrequentMedications { get; set; } = new List<string>();
        public List<PriorityPoint> PriorityEvolution { get; set; } = new List<PriorityPoint>();
    }
}
