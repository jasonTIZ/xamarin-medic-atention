using MedicalAtention.API.Models;
using Microsoft.EntityFrameworkCore;

namespace MedicalAtention.API.Data;

public static class DbInitializer
{
    public static void Seed(AppDbContext context)
    {
        context.Database.EnsureCreated();
        EnsurePatientsTable(context);

        if (!context.Users.Any())
        {
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
                });
            context.SaveChanges();
        }

        if (!context.Patients.Any())
        {
            var now = DateTime.UtcNow;
            context.Patients.AddRange(
                new Patient
                {
                    FirstName = "María",
                    LastName = "Rodríguez",
                    DocumentNumber = "1-2345-6789",
                    Gender = "Femenino",
                    PriorityLevel = 2,
                    CreatedAt = now
                },
                new Patient
                {
                    FirstName = "Juan",
                    LastName = "Pérez",
                    DocumentNumber = "2-3456-7890",
                    Gender = "Masculino",
                    PriorityLevel = 1,
                    CreatedAt = now
                },
                new Patient
                {
                    FirstName = "Ana",
                    LastName = "López",
                    DocumentNumber = "3-4567-8901",
                    Gender = "Femenino",
                    PriorityLevel = 0,
                    CreatedAt = now
                },
                new Patient
                {
                    FirstName = "Carlos",
                    LastName = "Méndez",
                    DocumentNumber = "4-5678-9012",
                    Gender = "Masculino",
                    PriorityLevel = 0,
                    CreatedAt = now
                });
            context.SaveChanges();
        }
    }

    /// <summary>
    /// Adds Patients table when upgrading an existing SQLite file that only had Users.
    /// </summary>
    private static void EnsurePatientsTable(AppDbContext context)
    {
        try
        {
            _ = context.Patients.Any();
        }
        catch
        {
            context.Database.ExecuteSqlRaw("""
                CREATE TABLE IF NOT EXISTS "Patients" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_Patients" PRIMARY KEY AUTOINCREMENT,
                    "FirstName" TEXT NOT NULL,
                    "LastName" TEXT NOT NULL,
                    "DocumentNumber" TEXT NULL,
                    "BirthDate" TEXT NULL,
                    "Gender" TEXT NULL,
                    "Address" TEXT NULL,
                    "Phone" TEXT NULL,
                    "PriorityLevel" INTEGER NOT NULL,
                    "CreatedAt" TEXT NOT NULL,
                    "UpdatedAt" TEXT NULL
                );
                """);
            context.Database.ExecuteSqlRaw("""
                CREATE INDEX IF NOT EXISTS "IX_Patients_DocumentNumber" ON "Patients" ("DocumentNumber");
                """);
        }
    }
}
