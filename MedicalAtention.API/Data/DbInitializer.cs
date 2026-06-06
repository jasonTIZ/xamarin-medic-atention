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
        EnsureDevicesTable(context);

        if (context.Users.Any()) return;

        var now = DateTime.UtcNow;

        context.Users.AddRange(
            new User { Name = "Administrador", Email = "admin@medic.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"), Role = "Admin", CreatedAt = now },
            new User { Name = "Dr. García", Email = "doctor@medic.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Doctor123!"), Role = "Doctor", CreatedAt = now }
        );
        context.SaveChanges();

        if (!context.Patients.Any())
        {
            var patients = new[]
            {
                new Patient { Name = "María", LastName = "López Ruiz", IdentificationNumber = "1712345678", DateOfBirth = new DateTime(1985, 3, 12), Gender = "Femenino", CreatedAt = now },
                new Patient { Name = "Carlos", LastName = "Mendoza Vera", IdentificationNumber = "0923456789", DateOfBirth = new DateTime(1970, 8, 25), Gender = "Masculino", CreatedAt = now },
                new Patient { Name = "Rosa", LastName = "Chávez Andrade", IdentificationNumber = "1534567890", DateOfBirth = new DateTime(1995, 11, 3), Gender = "Femenino", CreatedAt = now }
            };
            context.Patients.AddRange(patients);
            context.SaveChanges();

            context.Consultations.AddRange(
                // María López — evolución desde high hasta urgent
                new Consultation { PatientId = patients[0].Id, ConsultationDate = now.AddDays(-90), Symptoms = "Fiebre 39°C, cefalea intensa, mialgia generalizada", Diagnosis = "Síndrome gripal", Treatment = "Paracetamol 500mg cada 6 horas, Ibuprofeno 400mg cada 8 horas, hidratación oral", Notes = "Reposo por 5 días. Control si persiste fiebre.", Priority = "medium", CreatedAt = now.AddDays(-90) },
                new Consultation { PatientId = patients[0].Id, ConsultationDate = now.AddDays(-60), Symptoms = "Tos productiva con expectoración amarilla, fiebre 38.5°C, disnea leve", Diagnosis = "Neumonía bacteriana lóbulo inferior derecho", Treatment = "Amoxicilina-Clavulanato 875mg cada 12 horas por 10 días, Azitromicina 500mg día por 5 días, Salbutamol inhalador cada 6 horas", Notes = "Radiografía de tórax confirma infiltrado. Control en 7 días.", Priority = "high", CreatedAt = now.AddDays(-60) },
                new Consultation { PatientId = patients[0].Id, ConsultationDate = now.AddDays(-20), Symptoms = "Dolor torácico, disnea en reposo, saturación O2 88%", Diagnosis = "Insuficiencia respiratoria aguda sobre neumonía", Treatment = "Oxígeno suplementario 2L/min, Meropenem 1g IV cada 8 horas, Dexametasona 6mg IV, Enoxaparina 40mg SC", Notes = "Derivada a hospital de referencia.", Priority = "urgent", CreatedAt = now.AddDays(-20) },

                // Carlos Mendoza — hipertensión crónica con progresión
                new Consultation { PatientId = patients[1].Id, ConsultationDate = now.AddDays(-120), Symptoms = "PA 150/95, cefalea occipital matutina, tinitus", Diagnosis = "Hipertensión arterial grado I", Treatment = "Enalapril 10mg una vez al día, dieta hiposódica, ejercicio moderado", Notes = "Control mensual de presión arterial.", Priority = "medium", CreatedAt = now.AddDays(-120) },
                new Consultation { PatientId = patients[1].Id, ConsultationDate = now.AddDays(-75), Symptoms = "PA 165/100, mareos, visión borrosa episódica", Diagnosis = "Hipertensión arterial grado II, mal controlada", Treatment = "Enalapril 20mg, Amlodipino 5mg, Hidroclorotiazida 25mg, Aspirina 100mg", Notes = "Se ajusta dosis. Pedir perfil lipídico.", Priority = "high", CreatedAt = now.AddDays(-75) },
                new Consultation { PatientId = patients[1].Id, ConsultationDate = now.AddDays(-30), Symptoms = "PA 175/110, cefalea severa, náuseas, déficit visual", Diagnosis = "Crisis hipertensiva sin compromiso de órgano blanco", Treatment = "Captopril 25mg sublingual, reposo, Enalapril 20mg ajustado, Amlodipino 10mg", Notes = "Pendiente ecocardiograma. Control en 48 horas.", Priority = "urgent", CreatedAt = now.AddDays(-30) },
                new Consultation { PatientId = patients[1].Id, ConsultationDate = now.AddDays(-5), Symptoms = "PA 145/90, refiere mejor adherencia al tratamiento", Diagnosis = "Hipertensión arterial en control", Treatment = "Enalapril 20mg, Amlodipino 10mg, Hidroclorotiazida 25mg, Aspirina 100mg", Notes = "Ecocardiograma: hipertrofia ventricular leve.", Priority = "high", CreatedAt = now.AddDays(-5) },

                // Rosa Chávez — consultas agudas variadas
                new Consultation { PatientId = patients[2].Id, ConsultationDate = now.AddDays(-45), Symptoms = "Dolor en fosa iliaca derecha, náuseas, fiebre 37.8°C", Diagnosis = "Apendicitis aguda (sospecha)", Treatment = "Derivación urgente a cirugía, Metronidazol 500mg IV, Ceftriaxona 1g IV, NPO", Notes = "Confirmado por ecografía. Operada misma noche.", Priority = "urgent", CreatedAt = now.AddDays(-45) },
                new Consultation { PatientId = patients[2].Id, ConsultationDate = now.AddDays(-15), Symptoms = "Dolor epigástrico postoperatorio, náuseas, herida quirúrgica eritematosa", Diagnosis = "Infección superficial sitio quirúrgico", Treatment = "Cefalexina 500mg cada 6 horas por 7 días, Omeprazol 20mg en ayunas, curación local diaria", Notes = "Herida limpia y seca. Retiro de puntos en 5 días.", Priority = "medium", CreatedAt = now.AddDays(-15) },
                new Consultation { PatientId = patients[2].Id, ConsultationDate = now.AddDays(-3), Symptoms = "Control posquirúrgico, sin fiebre, herida cicatrizada", Diagnosis = "Posoperatorio apendicectomía, evolución favorable", Treatment = "Dieta normal progresiva, Paracetamol 500mg si dolor", Notes = "Alta médica. Próximo control en 30 días.", Priority = "low", CreatedAt = now.AddDays(-3) }
            );
            context.SaveChanges();
        }
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

    private static void EnsureDevicesTable(AppDbContext context)
    {
        context.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS Devices (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Token TEXT NOT NULL,
                Platform TEXT NOT NULL DEFAULT '',
                UserId INTEGER NOT NULL DEFAULT 0,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NULL
            );");
        context.Database.ExecuteSqlRaw(
            "CREATE UNIQUE INDEX IF NOT EXISTS IX_Devices_Token ON Devices (Token);");
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
