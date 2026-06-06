using MedicalAtention.API.Data;
using MedicalAtention.API.DTOs;
using MedicalAtention.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MedicalAtention.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DevicesController(AppDbContext db) : ControllerBase
{
    private static readonly HashSet<string> ValidPlatforms =
        new(StringComparer.OrdinalIgnoreCase) { "android", "ios" };

    [HttpPost("register")]
    public IActionResult Register([FromBody] DeviceRegisterRequest request)
    {
        var token = (request.Token ?? string.Empty).Trim();
        var platform = (request.Platform ?? string.Empty).Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(token))
            return BadRequest(new { field = "token", message = "El token es requerido" });

        if (!ValidPlatforms.Contains(platform))
            return BadRequest(new { field = "platform", message = "Plataforma inválida (android | ios)" });

        // El usuario autenticado (claim del JWT) es la fuente de verdad; se usa
        // el user_id del cuerpo solo como respaldo.
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userId = int.TryParse(userIdClaim, out var uid) ? uid : request.UserId;

        // Upsert por token: un mismo dispositivo no se duplica; se actualiza el dueño/plataforma.
        var device = db.Devices.FirstOrDefault(d => d.Token == token);
        if (device is null)
        {
            device = new Device
            {
                Token = token,
                Platform = platform,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            db.Devices.Add(device);
        }
        else
        {
            device.Platform = platform;
            device.UserId = userId;
            device.UpdatedAt = DateTime.UtcNow;
        }

        db.SaveChanges();

        return Ok(new
        {
            device.Id,
            device.Token,
            device.Platform,
            device.UserId
        });
    }
}
