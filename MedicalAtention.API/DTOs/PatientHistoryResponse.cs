namespace MedicalAtention.API.DTOs;

public record DiagnosisSummary(string Diagnosis, string Date);
public record PriorityPoint(string Date, string Priority);

public record PatientHistoryResponse(
    int PatientId,
    string FullName,
    int TotalConsultations,
    string? LastConsultationDate,
    string CurrentPriority,
    IEnumerable<DiagnosisSummary> RecentDiagnoses,
    IEnumerable<string> FrequentMedications,
    IEnumerable<PriorityPoint> PriorityEvolution
);
