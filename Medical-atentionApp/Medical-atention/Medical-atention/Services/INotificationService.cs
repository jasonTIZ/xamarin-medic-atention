using System;
using System.Threading.Tasks;

namespace Medical_atention.Services
{
    public interface INotificationService
    {
        // Programa una notificación local de seguimiento para el paciente.
        Task ScheduleFollowUpAsync(int patientId, string patientName, string reason, DateTime notifyTime);

        // Cancela todas las notificaciones de seguimiento pendientes del paciente.
        // Devuelve la cantidad cancelada.
        Task<int> CancelForPatientAsync(int patientId);

        // Cantidad de recordatorios pendientes (programados a futuro) del paciente.
        Task<int> GetPendingCountAsync(int patientId);
    }
}
