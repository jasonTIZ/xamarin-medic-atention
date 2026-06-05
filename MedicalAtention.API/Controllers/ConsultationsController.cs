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
    [HttpPost]
    public IActionResult Create([FromBody] ConsultationRequestDto request)
    {
        var patient = db.Patients.Find(request.PatientId);
        if (patient is null) return NotFound(new { message = "Paciente no encontrado" });

        var consultation = new Consultation
        {
            PatientId = request.PatientId,
            ConsultationDate = request.ConsultationDate,
            Symptoms = request.Symptoms ?? string.Empty,
            Diagnosis = request.Diagnosis ?? string.Empty,
            Treatment = request.Treatment ?? string.Empty,
            Notes = request.Notes ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        patient.LastConsultationAt = consultation.ConsultationDate;
        patient.UpdatedAt = DateTime.UtcNow;

        db.Consultations.Add(consultation);
        db.SaveChanges();

        return CreatedAtAction(nameof(GetById), new { id = consultation.Id }, ToDto(consultation));
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var consultation = db.Consultations.Find(id);
        if (consultation is null) return NotFound();
        return Ok(ToDto(consultation));
    }

    private static ConsultationResponseDto ToDto(Consultation c) =>
        new(c.Id, c.PatientId, c.ConsultationDate, c.Symptoms, c.Diagnosis, c.Treatment, c.Notes, c.CreatedAt);
}
