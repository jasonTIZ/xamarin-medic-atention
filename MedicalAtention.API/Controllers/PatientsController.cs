using MedicalAtention.API.Data;
using MedicalAtention.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalAtention.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class PatientsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PatientDto>>> GetAll(CancellationToken cancellationToken)
    {
        var patients = await db.Patients
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .Select(p => new PatientDto(
                p.Id,
                p.FirstName,
                p.LastName,
                $"{p.FirstName} {p.LastName}".Trim(),
                p.DocumentNumber,
                p.PriorityLevel))
            .ToListAsync(cancellationToken);

        return Ok(patients);
    }
}
