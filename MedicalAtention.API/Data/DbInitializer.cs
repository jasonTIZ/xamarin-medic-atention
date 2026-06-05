using MedicalAtention.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace MedicalAtention.API.Data;

public static class DbInitializer
{
    public static void Seed(AppDbContext context)
    {
        EnsureConsultationsTable(context);
        EnsurePatientPriorityColumns(context);
        EnsureAttachmentsTable(context);

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

    public static void SeedPatientsIfEmpty(AppDbContext context)
    {
        EnsurePatientPriorityColumns(context);

        if (context.Patients.Any()) return;

        var now = DateTime.UtcNow;
        context.Patients.AddRange(
            new Patient
            {
                Name = "Ana", LastName = "López", IdentificationNumber = "123456789",
                DateOfBirth = new DateTime(1985, 3, 15), Gender = "Femenino",
                Priority = PriorityLevel.Urgent,
                LastConsultationAt = now.AddHours(-2),
                CreatedAt = now
            },
            new Patient
            {
                Name = "Carlos", LastName = "Ramírez", IdentificationNumber = "987654321",
                DateOfBirth = new DateTime(1978, 7, 22), Gender = "Masculino",
                Priority = PriorityLevel.High,
                LastConsultationAt = now.AddDays(-1),
                CreatedAt = now
            },
            new Patient
            {
                Name = "María", LastName = "González", IdentificationNumber = "456789123",
                DateOfBirth = new DateTime(1992, 11, 8), Gender = "Femenino",
                Priority = PriorityLevel.Medium,
                LastConsultationAt = now.AddDays(-5),
                CreatedAt = now
            },
            new Patient
            {
                Name = "José", LastName = "Martínez", IdentificationNumber = "789123456",
                DateOfBirth = new DateTime(1965, 1, 30), Gender = "Masculino",
                Priority = PriorityLevel.Low,
                LastConsultationAt = null,
                CreatedAt = now
            }
        );

        context.SaveChanges();
    }

    private static void EnsureAttachmentsTable(AppDbContext context)
    {
        context.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS Attachments (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ConsultationId INTEGER NOT NULL,
                FileName TEXT NOT NULL DEFAULT '',
                StoragePath TEXT NOT NULL DEFAULT '',
                FileSizeBytes INTEGER NOT NULL DEFAULT 0,
                ContentType TEXT NOT NULL DEFAULT '',
                UploadedAt TEXT NOT NULL,
                FOREIGN KEY (ConsultationId) REFERENCES Consultations(Id) ON DELETE CASCADE
            );");
    }

    private static void EnsureConsultationsTable(AppDbContext context)
    {
        context.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS Consultations (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                PatientId INTEGER NOT NULL,
                ConsultationDate TEXT NOT NULL,
                Symptoms TEXT NOT NULL DEFAULT '',
                Diagnosis TEXT NOT NULL DEFAULT '',
                Treatment TEXT NOT NULL DEFAULT '',
                Notes TEXT NOT NULL DEFAULT '',
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NULL,
                FOREIGN KEY (PatientId) REFERENCES Patients(Id) ON DELETE CASCADE
            );");
    }

    private static void EnsurePatientPriorityColumns(AppDbContext context)
    {
        var connection = context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            connection.Open();

        var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = "PRAGMA table_info(Patients)";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                columns.Add(reader.GetString(1));
        }

        if (!columns.Contains("Priority"))
        {
            context.Database.ExecuteSqlRaw(
                "ALTER TABLE Patients ADD COLUMN Priority INTEGER NOT NULL DEFAULT 2");
        }

        if (!columns.Contains("LastConsultationAt"))
        {
            context.Database.ExecuteSqlRaw(
                "ALTER TABLE Patients ADD COLUMN LastConsultationAt TEXT NULL");
        }
    }
}
