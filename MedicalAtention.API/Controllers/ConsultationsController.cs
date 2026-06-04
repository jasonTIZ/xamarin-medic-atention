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

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var consultation = db.Consultations.Find(id);
        if (consultation is null) return NotFound();

        return Ok(new ConsultationResponseDto(
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
}
