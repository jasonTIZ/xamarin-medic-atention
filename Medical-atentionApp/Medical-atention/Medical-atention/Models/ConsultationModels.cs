using System;

namespace Medical_atention.Models
{
    public class ConsultationRequestDto
    {
        public int PatientId { get; set; }
        public DateTime ConsultationDate { get; set; }
        public string Symptoms { get; set; }
        public string Diagnosis { get; set; }
        public string Treatment { get; set; }
        public string Notes { get; set; }
        public string Priority { get; set; }
    }

    public class ConsultationResponseDto
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public DateTime ConsultationDate { get; set; }
        public string Symptoms { get; set; }
        public string Diagnosis { get; set; }
        public string Treatment { get; set; }
        public string Notes { get; set; }
        public string Priority { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
