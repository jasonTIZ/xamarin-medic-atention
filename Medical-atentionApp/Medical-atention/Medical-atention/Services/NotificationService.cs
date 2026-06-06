using Medical_atention.Data;
using Medical_atention.Models;
using Medical_atention.Models.Entities;
using Plugin.LocalNotification;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Medical_atention.Services
{
    // Programa y cancela recordatorios de seguimiento usando Plugin.LocalNotification.
    // El recordatorio siempre se persiste localmente (tabla reminders) para poder
    // listarlo y cancelarlo aunque la programación en el SO falle o no esté disponible.
    public class NotificationService : INotificationService
    {
        private const string FollowUpTitlePrefix = "Seguimiento";
        private readonly IReminderRepository _repository;

        public NotificationService() : this(new ReminderRepository()) { }

        public NotificationService(IReminderRepository repository)
        {
            _repository = repository;
        }

        public async Task ScheduleFollowUpAsync(int patientId, string patientName, string reason, DateTime notifyTime)
        {
            patientName = string.IsNullOrWhiteSpace(patientName) ? $"Paciente #{patientId}" : patientName.Trim();
            reason = string.IsNullOrWhiteSpace(reason) ? "Consulta urgente" : reason.Trim();

            var reminder = new ReminderEntity
            {
                PatientId = patientId,
                PatientName = patientName,
                Reason = reason,
                NotifyTime = notifyTime,
                Status = ReminderStatus.Scheduled,
                CreatedAt = DateTime.UtcNow
            };

            var notificationId = await _repository.AddAsync(reminder);

            try
            {
                var center = NotificationCenter.Current;
                if (center == null) return;

                var request = new NotificationRequest
                {
                    NotificationId = notificationId,
                    Title = $"{FollowUpTitlePrefix} — {patientName}",
                    Description = $"Motivo: {reason}",
                    ReturningData = patientId.ToString(),
                    Schedule = new NotificationRequestSchedule
                    {
                        NotifyTime = notifyTime
                    }
                };

                await center.Show(request);
            }
            catch (Exception ex)
            {
                // El recordatorio queda persistido localmente; solo falló la programación en el SO.
                Debug.WriteLine($"[NotificationService] No se pudo programar la notificación: {ex.Message}");
            }
        }

        public async Task<int> CancelForPatientAsync(int patientId)
        {
            var scheduled = await _repository.GetScheduledByPatientAsync(patientId);
            var cancelled = 0;

            foreach (var reminder in scheduled)
            {
                try
                {
                    NotificationCenter.Current?.Cancel(reminder.Id);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[NotificationService] No se pudo cancelar la notificación {reminder.Id}: {ex.Message}");
                }

                await _repository.MarkCancelledAsync(reminder.Id);
                cancelled++;
            }

            return cancelled;
        }

        public async Task<int> GetPendingCountAsync(int patientId)
        {
            var scheduled = await _repository.GetScheduledByPatientAsync(patientId);
            return scheduled.Count(r => r.NotifyTime > DateTime.Now);
        }
    }
}
