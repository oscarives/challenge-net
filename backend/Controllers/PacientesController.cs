using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
    public async Task<IActionResult> Create([FromBody] Paciente paciente)
    {
        paciente.createdAt = DateTime.UtcNow;
        paciente.isActive = true;
        _context.Pacientes.Add(paciente);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = paciente.Id }, ToResponse(paciente, DateTime.UtcNow));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Paciente paciente)
    {
        var existing = await _context.Pacientes.FindAsync(id);
        if (existing == null) return NotFound();

        existing.NombreCompleto = paciente.NombreCompleto;
        existing.DNI = paciente.DNI;
        existing.Email = paciente.Email;
        existing.Telefono = paciente.Telefono;

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
