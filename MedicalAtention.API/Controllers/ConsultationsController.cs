using MedicalAtention.API.Data;
using MedicalAtention.API.DTOs;
using MedicalAtention.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicalAtention.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConsultationsController(AppDbContext db) : ControllerBase
{
    private static readonly HashSet<string> ValidPriorities = new(StringComparer.OrdinalIgnoreCase)
        { "urgent", "high", "medium", "low" };

    [HttpPost]
    public IActionResult Create([FromBody] ConsultationRequestDto request)
    {
        if (request.PatientId <= 0)
            return BadRequest(new { field = "patientId", message = "Paciente inválido" });

        if (db.Patients.Find(request.PatientId) is null)
            return NotFound(new { field = "patientId", message = "Paciente no encontrado" });

        if (string.IsNullOrWhiteSpace(request.Symptoms))
            return BadRequest(new { field = "symptoms", message = "Los síntomas son requeridos" });

        if (string.IsNullOrWhiteSpace(request.Diagnosis))
            return BadRequest(new { field = "diagnosis", message = "El diagnóstico es requerido" });

        if (!ValidPriorities.Contains(request.Priority))
            return BadRequest(new { field = "priority", message = "Prioridad inválida" });

        var consultation = new Consultation
        {
            PatientId = request.PatientId,
            ConsultationDate = request.ConsultationDate,
            Symptoms = request.Symptoms.Trim(),
            Diagnosis = request.Diagnosis.Trim(),
            Treatment = request.Treatment?.Trim() ?? string.Empty,
            Notes = request.Notes?.Trim() ?? string.Empty,
            Priority = request.Priority.Trim().ToLowerInvariant(),
            CreatedAt = DateTime.UtcNow
        };

        db.Consultations.Add(consultation);
        db.SaveChanges();

        return CreatedAtAction(nameof(GetById), new { id = consultation.Id },
            new ConsultationResponseDto(
                consultation.Id,
                consultation.PatientId,
                consultation.ConsultationDate,
                consultation.Symptoms,
                consultation.Diagnosis,
                consultation.Treatment,
                consultation.Notes,
                consultation.Priority,
                consultation.CreatedAt));
    }

    [HttpGet]
    public IActionResult GetByPatient(
        [FromQuery(Name = "patient_id")] int patientId,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        if (patientId <= 0)
            return BadRequest(new { field = "patient_id", message = "Paciente inválido" });

        if (db.Patients.Find(patientId) is null)
            return NotFound(new { field = "patient_id", message = "Paciente no encontrado" });

        var query = db.Consultations.Where(c => c.PatientId == patientId);

        if (from.HasValue)
            query = query.Where(c => c.ConsultationDate >= from.Value.Date);

        if (to.HasValue)
        {
            var end = to.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(c => c.ConsultationDate <= end);
        }

        var list = query
            .OrderByDescending(c => c.ConsultationDate)
            .Select(c => new ConsultationResponseDto(
                c.Id,
                c.PatientId,
                c.ConsultationDate,
                c.Symptoms,
                c.Diagnosis,
                c.Treatment,
                c.Notes,
                c.Priority,
                c.CreatedAt))
            .ToList();

        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var consultation = db.Consultations.Find(id);
        if (consultation is null) return NotFound();

        return Ok(MapConsultation(consultation));
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] ConsultationUpdateDto request)
    {
        var consultation = db.Consultations.Find(id);
        if (consultation is null) return NotFound();

        consultation.Treatment = request.Treatment?.Trim() ?? string.Empty;
        consultation.Notes = request.Notes?.Trim() ?? string.Empty;
        db.SaveChanges();

        return Ok(MapConsultation(consultation));
    }

    private static ConsultationResponseDto MapConsultation(Consultation consultation)
        => new(
            consultation.Id,
            consultation.PatientId,
            consultation.ConsultationDate,
            consultation.Symptoms,
            consultation.Diagnosis,
            consultation.Treatment,
            consultation.Notes,
            consultation.Priority,
            consultation.CreatedAt);
}
