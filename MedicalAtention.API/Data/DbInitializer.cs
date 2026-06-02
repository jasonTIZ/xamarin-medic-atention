using MedicalAtention.API.Models;

namespace MedicalAtention.API.Data;

public static class DbInitializer
{
    public static void Seed(AppDbContext context)
    {
        if (context.Users.Any()) return;

        var now = DateTime.UtcNow;
        context.Users.AddRange(
            new User
            {
                Name = "Administrador",
                Email = "admin@medic.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role = "Admin",
                CreatedAt = now
            },
            new User
            {
                Name = "Dr. García",
                Email = "doctor@medic.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Doctor123!"),
                Role = "Doctor",
                CreatedAt = now
            }
        );

        context.SaveChanges();
    }
}
