namespace MedicalAtention.API.Models;

// Dispositivo registrado para recibir notificaciones push (FCM/APNs).
public class Device
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;   // FCM token (Android) o APNs/FCM (iOS)
    public string Platform { get; set; } = string.Empty; // "android" | "ios"
    public int UserId { get; set; }                      // Usuario propietario del dispositivo
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
