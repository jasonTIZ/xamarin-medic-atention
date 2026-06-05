using MedicalAtention.API.Data;
using MedicalAtention.API.DTOs;
using MedicalAtention.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using System.Linq;

namespace MedicalAtention.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PatientsController(AppDbContext db) : ControllerBase
{
    private static readonly Regex IdentificationNumberRegex = new(@"^\d{9,12}$");

    [HttpPost]
    public IActionResult Create([FromBody] PatientRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { field = "name", message = "El nombre es requerido" });

        if (string.IsNullOrWhiteSpace(request.LastName))
            return BadRequest(new { field = "lastName", message = "El apellido es requerido" });

        if (string.IsNullOrWhiteSpace(request.IdentificationNumber) || !IdentificationNumberRegex.IsMatch(request.IdentificationNumber))
            return BadRequest(new { field = "cedula", message = "Formato de cédula inválido (9 a 12 dígitos)" });

        if (request.DateOfBirth.Date >= DateTime.UtcNow.Date)
            return BadRequest(new { field = "dateOfBirth", message = "La fecha de nacimiento no puede ser futura" });

        if (db.Patients.Any(p => p.IdentificationNumber == request.IdentificationNumber))
            return Conflict(new { field = "cedula", message = "Ya existe un paciente con esta cédula" });

        var patient = new Patient
        {
            Name = request.Name.Trim(),
            LastName = request.LastName.Trim(),
            IdentificationNumber = request.IdentificationNumber.Trim(),
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            CreatedAt = DateTime.UtcNow
        };

        db.Patients.Add(patient);
        db.SaveChanges();

        return CreatedAtAction(nameof(GetById), new { id = patient.Id }, MapPatient(patient));
    }

    [HttpGet]
    public IActionResult GetAll()
        => Ok(db.Patients.AsEnumerable().Select(MapPatient));

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var patient = db.Patients.Find(id);
        if (patient is null) return NotFound();
        return Ok(MapPatient(patient));
    }

    private PatientResponseDto MapPatient(Patient patient)
    {
        var last = db.Consultations
            .Where(c => c.PatientId == patient.Id)
            .OrderByDescending(c => c.ConsultationDate)
            .FirstOrDefault();

        return new PatientResponseDto(
            patient.Id,
            patient.Name,
            patient.LastName,
            patient.IdentificationNumber,
            patient.DateOfBirth,
            patient.Gender,
            patient.CreatedAt,
            last?.Priority,
            last?.ConsultationDate);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] PatientRequestDto request)
    {
        var patient = db.Patients.Find(id);
        if (patient is null) return NotFound();

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { field = "name", message = "El nombre es requerido" });

        if (string.IsNullOrWhiteSpace(request.LastName))
            return BadRequest(new { field = "lastName", message = "El apellido es requerido" });

        if (string.IsNullOrWhiteSpace(request.IdentificationNumber) || !IdentificationNumberRegex.IsMatch(request.IdentificationNumber))
            return BadRequest(new { field = "cedula", message = "Formato de cédula inválido (9 a 12 dígitos)" });

        if (request.DateOfBirth.Date >= DateTime.UtcNow.Date)
            return BadRequest(new { field = "dateOfBirth", message = "La fecha de nacimiento no puede ser futura" });

        if (db.Patients.Any(p => p.IdentificationNumber == request.IdentificationNumber && p.Id != id))
            return Conflict(new { field = "cedula", message = "Ya existe un paciente con esta cédula" });

        patient.Name = request.Name.Trim();
        patient.LastName = request.LastName.Trim();
        patient.IdentificationNumber = request.IdentificationNumber.Trim();
        patient.DateOfBirth = request.DateOfBirth;
        patient.Gender = request.Gender;
        patient.UpdatedAt = DateTime.UtcNow;
        db.SaveChanges();

        return Ok(MapPatient(patient));
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var patient = db.Patients.Find(id);
        if (patient is null) return NotFound();
        db.Patients.Remove(patient);
        db.SaveChanges();
        return NoContent();
    }

    [HttpGet("{id}/history")]
    public IActionResult GetHistory(int id)
    {
        var patient = db.Patients.Find(id);
        if (patient is null)
            return NotFound(new { message = "Paciente no encontrado" });

        var consultations = db.Consultations
            .Where(c => c.PatientId == id)
            .OrderByDescending(c => c.ConsultationDate)
            .ToList();

        var recentDiagnoses = consultations
            .Where(c => !string.IsNullOrWhiteSpace(c.Diagnosis))
            .Take(5)
            .Select(c => new DiagnosisSummary(c.Diagnosis, c.ConsultationDate.ToString("yyyy-MM-dd")));

        var priorityEvolution = consultations
            .OrderBy(c => c.ConsultationDate)
            .Select(c => new PriorityPoint(c.ConsultationDate.ToString("yyyy-MM-dd"), c.Priority));

        return Ok(new PatientHistoryResponse(
            patient.Id,
            $"{patient.Name} {patient.LastName}",
            consultations.Count,
            consultations.FirstOrDefault()?.ConsultationDate.ToString("yyyy-MM-dd"),
            consultations.FirstOrDefault()?.Priority ?? "medium",
            recentDiagnoses,
            ExtractMedications(consultations.Select(c => c.Treatment)),
            priorityEvolution
        ));
    }

    private static IEnumerable<string> ExtractMedications(IEnumerable<string> treatments)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var treatment in treatments.Where(t => !string.IsNullOrWhiteSpace(t)))
        {
            foreach (var segment in treatment.Split(new[] { ',', ';', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var entry = segment.Trim();
                if (entry.Length >= 4 && entry.Length <= 80)
                    seen.Add(entry);
            }
        }
        return seen.Take(10);
    }
}
