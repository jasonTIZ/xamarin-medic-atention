using MedicalAtention.API.Data;
using MedicalAtention.API.DTOs;
using MedicalAtention.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

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
            Priority = PriorityLevel.Medium,
            CreatedAt = DateTime.UtcNow
        };

        db.Patients.Add(patient);
        db.SaveChanges();

        return CreatedAtAction(nameof(GetById), new { id = patient.Id }, ToDto(patient));
    }

    [HttpGet]
    public IActionResult GetAll([FromQuery] string? sort)
    {
        var query = db.Patients.AsQueryable();

        if (string.Equals(sort, "priority", StringComparison.OrdinalIgnoreCase))
        {
            query = query
                .OrderBy(p => p.Priority)
                .ThenBy(p => p.LastName)
                .ThenBy(p => p.Name);
        }
        else
        {
            query = query.OrderBy(p => p.LastName).ThenBy(p => p.Name);
        }

        return Ok(query.Select(p => ToDto(p)).ToList());
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var patient = db.Patients.Find(id);
        if (patient is null) return NotFound();
        return Ok(ToDto(patient));
    }

    [HttpPatch("{id}/priority")]
    public IActionResult UpdatePriority(int id, [FromBody] UpdatePatientPriorityRequest request)
    {
        var patient = db.Patients.Find(id);
        if (patient is null) return NotFound();

        patient.Priority = request.Priority;
        patient.UpdatedAt = DateTime.UtcNow;
        db.SaveChanges();

        return Ok(ToDto(patient));
    }

    private static PatientResponseDto ToDto(Patient p) =>
        new(p.Id, p.Name, p.LastName, p.IdentificationNumber, p.DateOfBirth, p.Gender,
            p.Priority, p.LastConsultationAt, p.CreatedAt);
}
