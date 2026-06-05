namespace Medical_atention.Models
{
    public static class SyncEntityType
    {
        public const string Patient = "Patient";
        public const string Consultation = "Consultation";
    }

    public static class SyncOperation
    {
        public const string CreatePatient = "CreatePatient";
        public const string UpdatePatient = "UpdatePatient";
        public const string DeletePatient = "DeletePatient";
        public const string UpdatePatientPriority = "UpdatePatientPriority";
        public const string CreateConsultation = "CreateConsultation";
    }
}
