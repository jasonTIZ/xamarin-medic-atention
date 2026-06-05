using MedicalAtention.API.Data;
using MedicalAtention.API.DTOs;
using MedicalAtention.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace MedicalAtention.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConsultationsController(AppDbContext db, IWebHostEnvironment env) : ControllerBase
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
        { "image/jpeg", "image/jpg", "image/png" };
    private const int MaxAttachmentsPerConsultation = 3;
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB
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

    [HttpPost("{id:int}/attachments")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(16 * 1024 * 1024)]
    public async Task<IActionResult> UploadAttachments(int id, [FromForm] IFormFileCollection files)
    {
        var consultation = db.Consultations.Find(id);
        if (consultation is null) return NotFound(new { message = "Consulta no encontrada" });

        if (files is null || files.Count == 0)
            return BadRequest(new { message = "No se recibieron archivos" });

        var existing = db.Attachments.Count(a => a.ConsultationId == id);
        if (existing + files.Count > MaxAttachmentsPerConsultation)
            return BadRequest(new { message = $"Máximo {MaxAttachmentsPerConsultation} imágenes por consulta" });

        var uploadDir = Path.Combine(env.ContentRootPath, "uploads", "attachments", id.ToString());
        Directory.CreateDirectory(uploadDir);

        var created = new List<AttachmentResponseDto>();

        foreach (var file in files)
        {
            if (!AllowedContentTypes.Contains(file.ContentType))
                return BadRequest(new { message = $"Tipo de archivo no permitido: {file.ContentType}" });

            if (file.Length > MaxFileSizeBytes)
                return BadRequest(new { message = "Cada imagen debe ser menor a 5 MB" });

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png") ext = ".jpg";

            var storedName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(uploadDir, storedName);

            using (var stream = System.IO.File.Create(fullPath))
                await file.CopyToAsync(stream);

            var attachment = new ConsultationAttachment
            {
                ConsultationId = id,
                FileName = file.FileName,
                StoragePath = Path.Combine("attachments", id.ToString(), storedName),
                FileSizeBytes = file.Length,
                ContentType = file.ContentType,
                UploadedAt = DateTime.UtcNow
            };

            db.Attachments.Add(attachment);
            db.SaveChanges();

            created.Add(MapAttachment(attachment, Request));
        }

        return Ok(created);
    }

    [HttpGet("{id:int}/attachments")]
    public IActionResult GetAttachments(int id)
    {
        if (db.Consultations.Find(id) is null)
            return NotFound(new { message = "Consulta no encontrada" });

        var list = db.Attachments
            .Where(a => a.ConsultationId == id)
            .OrderBy(a => a.UploadedAt)
            .Select(a => MapAttachment(a, Request))
            .ToList();

        return Ok(list);
    }

    [HttpDelete("{id:int}/attachments/{attachmentId:int}")]
    public IActionResult DeleteAttachment(int id, int attachmentId)
    {
        var attachment = db.Attachments.FirstOrDefault(a => a.Id == attachmentId && a.ConsultationId == id);
        if (attachment is null) return NotFound();

        var fullPath = Path.Combine(env.ContentRootPath, "uploads", attachment.StoragePath);
        if (System.IO.File.Exists(fullPath))
            System.IO.File.Delete(fullPath);

        db.Attachments.Remove(attachment);
        db.SaveChanges();
        return NoContent();
    }

    private static AttachmentResponseDto MapAttachment(ConsultationAttachment a, HttpRequest req)
    {
        var url = $"{req.Scheme}://{req.Host}/uploads/{a.StoragePath.Replace('\\', '/')}";
        return new AttachmentResponseDto(a.Id, a.ConsultationId, a.FileName, url, a.FileSizeBytes, a.UploadedAt);
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
