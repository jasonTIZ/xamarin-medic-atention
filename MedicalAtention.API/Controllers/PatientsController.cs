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
            CreatedAt = DateTime.UtcNow
        };

        db.Patients.Add(patient);
        db.SaveChanges();

        return CreatedAtAction(nameof(GetById), new { id = patient.Id },
            new PatientResponseDto(patient.Id, patient.Name, patient.LastName, patient.IdentificationNumber, patient.DateOfBirth, patient.Gender, patient.CreatedAt));
    }

    [HttpGet]
    public IActionResult GetAll()
        => Ok(db.Patients.Select(p => new PatientResponseDto(p.Id, p.Name, p.LastName, p.IdentificationNumber, p.DateOfBirth, p.Gender, p.CreatedAt)));

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var patient = db.Patients.Find(id);
        if (patient is null) return NotFound();
        return Ok(new PatientResponseDto(patient.Id, patient.Name, patient.LastName, patient.IdentificationNumber, patient.DateOfBirth, patient.Gender, patient.CreatedAt));
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

        return Ok(new PatientResponseDto(patient.Id, patient.Name, patient.LastName, patient.IdentificationNumber, patient.DateOfBirth, patient.Gender, patient.CreatedAt));
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
}
