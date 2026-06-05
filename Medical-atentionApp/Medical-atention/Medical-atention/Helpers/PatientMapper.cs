using Medical_atention.Models;
using Medical_atention.Models.Entities;
using System;

namespace Medical_atention.Helpers
{
    public static class PatientMapper
    {
        public static Patient ToDomain(PatientEntity entity) =>
            new Patient
            {
                Id = entity.Id,
                LocalId = entity.LocalId,
                PendingSync = entity.PendingSync,
                FirstName = entity.FirstName ?? string.Empty,
                LastName = entity.LastName ?? string.Empty,
                DocumentNumber = entity.DocumentNumber ?? string.Empty,
                DateOfBirth = entity.DateOfBirth,
                Gender = entity.Gender ?? string.Empty,
                Priority = entity.Priority,
                LastConsultationAt = entity.LastConsultationAt,
                PendingPrioritySync = entity.PendingPrioritySync,
                PendingPriority = entity.PendingPriority
            };

        public static Patient FromDto(PatientResponseDto dto) =>
            new Patient
            {
                Id = dto.Id,
                FirstName = dto.Name ?? string.Empty,
                LastName = dto.LastName ?? string.Empty,
                DocumentNumber = dto.IdentificationNumber ?? string.Empty,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender ?? string.Empty,
                Priority = ParsePriority(dto.Priority),
                LastConsultationAt = dto.LastConsultationDate,
                PendingSync = false,
                PendingPrioritySync = false
            };

        public static PatientEntity ToEntity(Patient patient) =>
            new PatientEntity
            {
                Id = patient.Id,
                LocalId = patient.LocalId == Guid.Empty ? Guid.NewGuid() : patient.LocalId,
                PendingSync = patient.PendingSync,
                FirstName = patient.FirstName ?? string.Empty,
                LastName = patient.LastName ?? string.Empty,
                DocumentNumber = patient.DocumentNumber ?? string.Empty,
                DateOfBirth = patient.DateOfBirth,
                Gender = patient.Gender ?? string.Empty,
                Priority = patient.Priority,
                LastConsultationAt = patient.LastConsultationAt,
                PendingPrioritySync = patient.PendingPrioritySync,
                PendingPriority = patient.PendingPriority
            };

        public static PatientEntity FromDtoToEntity(PatientResponseDto dto) =>
            ToEntity(FromDto(dto));

        public static int ParsePriority(string priority)
        {
            switch (priority?.Trim().ToLowerInvariant())
            {
                case "urgent": return (int)PriorityLevel.Urgent;
                case "high": return (int)PriorityLevel.High;
                case "low": return (int)PriorityLevel.Low;
                default: return (int)PriorityLevel.Medium;
            }
        }

        public static string ToPriorityString(PriorityLevel level)
        {
            switch (level)
            {
                case PriorityLevel.Urgent: return "urgent";
                case PriorityLevel.High: return "high";
                case PriorityLevel.Low: return "low";
                default: return "medium";
            }
        }
    }
}
