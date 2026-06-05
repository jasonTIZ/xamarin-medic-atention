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
    }
}
