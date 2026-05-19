using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using TurnosMedicos.Data;
using TurnosMedicos.Models;
using TurnosMedicos.Services;

namespace TurnosMedicos.Controllers;

[ApiController]
[Route("[controller]")]
public class PacientesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly NoShowPenaltySettings _noShowPenaltySettings;

    public PacientesController(AppDbContext context, IOptions<NoShowPenaltySettings> noShowPenaltySettings)
    {
        _context = context;
        _noShowPenaltySettings = NoShowPenaltySettings.WithDefaults(noShowPenaltySettings.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var pacientes = await _context.Pacientes.ToListAsync();
        var now = DateTime.UtcNow;
        return Ok(pacientes.Select(p => ToResponse(p, now)));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var paciente = await _context.Pacientes.FindAsync(id);
        if (paciente == null) return NotFound();

        return Ok(ToResponse(paciente, DateTime.UtcNow));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UpsertPacienteRequest request)
    {
        var validationError = ValidateUpsertPacienteRequest(request);
        if (validationError != null)
            return BadRequest(new { mensaje = validationError });

        var paciente = new Paciente
        {
            NombreCompleto = request.NombreCompleto.Trim(),
            DNI = request.DNI.Trim(),
            Email = request.Email.Trim(),
            Telefono = request.Telefono.Trim()
        };

        paciente.createdAt = DateTime.UtcNow;
        paciente.isActive = true;
        _context.Pacientes.Add(paciente);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = paciente.Id }, ToResponse(paciente, DateTime.UtcNow));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpsertPacienteRequest request)
    {
        var validationError = ValidateUpsertPacienteRequest(request);
        if (validationError != null)
            return BadRequest(new { mensaje = validationError });

        var existing = await _context.Pacientes.FindAsync(id);
        if (existing == null) return NotFound();

        existing.NombreCompleto = request.NombreCompleto.Trim();
        existing.DNI = request.DNI.Trim();
        existing.Email = request.Email.Trim();
        existing.Telefono = request.Telefono.Trim();

        await _context.SaveChangesAsync();
        return Ok(ToResponse(existing, DateTime.UtcNow));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var paciente = await _context.Pacientes.FindAsync(id);
        if (paciente == null) return NotFound();

        _context.Pacientes.Remove(paciente);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static string? ValidateUpsertPacienteRequest(UpsertPacienteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.NombreCompleto))
            return "El nombre del paciente es obligatorio.";

        var nombre = request.NombreCompleto.Trim();
        if (nombre.Length < 3 || nombre.Length > 120)
            return "El nombre del paciente debe tener entre 3 y 120 caracteres.";

        if (string.IsNullOrWhiteSpace(request.DNI))
            return "El DNI es obligatorio.";

        var dni = request.DNI.Trim();
        if (dni.Length < 7 || dni.Length > 15)
            return "El DNI debe tener entre 7 y 15 caracteres.";

        if (string.IsNullOrWhiteSpace(request.Email))
            return "El email es obligatorio.";

        try
        {
            _ = new MailAddress(request.Email.Trim());
        }
        catch
        {
            return "El email no tiene un formato válido.";
        }

        if (string.IsNullOrWhiteSpace(request.Telefono))
            return "El teléfono es obligatorio.";

        var telefono = request.Telefono.Trim();
        if (telefono.Length < 6 || telefono.Length > 20)
            return "El teléfono debe tener entre 6 y 20 caracteres.";

        return null;
    }

    private PacienteResponse ToResponse(Paciente paciente, DateTime nowUtc)
    {
        var bloqueoHastaUtc = paciente.FechaBloqueo?.AddDays(_noShowPenaltySettings.BloqueoDias!.Value);
        var bloqueadoVigente = bloqueoHastaUtc.HasValue && bloqueoHastaUtc.Value > nowUtc;

        return new PacienteResponse
        {
            Id = paciente.Id,
            NombreCompleto = paciente.NombreCompleto,
            DNI = paciente.DNI,
            Email = paciente.Email,
            Telefono = paciente.Telefono,
            FechaBloqueo = paciente.FechaBloqueo,
            CreatedAt = paciente.createdAt,
            IsActive = paciente.isActive,
            BloqueadoVigente = bloqueadoVigente,
            BloqueoHastaUtc = bloqueadoVigente ? bloqueoHastaUtc : null
        };
    }
}

public class PacienteResponse
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string DNI { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public DateTime? FechaBloqueo { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public bool BloqueadoVigente { get; set; }
    public DateTime? BloqueoHastaUtc { get; set; }
}

public class UpsertPacienteRequest
{
    public string NombreCompleto { get; set; } = string.Empty;
    public string DNI { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
}
