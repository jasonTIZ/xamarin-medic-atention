using Medical_atention.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Medical_atention.Data
{
    public interface IReminderRepository
    {
        // Inserta el recordatorio y devuelve el Id generado (usado como NotificationId).
        Task<int> AddAsync(ReminderEntity reminder);

        // Recordatorios en estado Scheduled para un paciente.
        Task<List<ReminderEntity>> GetScheduledByPatientAsync(int patientId);

        // Marca un recordatorio como Cancelled.
        Task MarkCancelledAsync(int id);
    }
}
