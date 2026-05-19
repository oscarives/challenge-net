using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TurnosMedicos.Data;
using TurnosMedicos.Helpers;
using TurnosMedicos.Models;
using TurnosMedicos.Services;

namespace TurnosMedicos.Controllers;

[ApiController]
[Route("[controller]")]
public class TurnosController : ControllerBase
{
    private static readonly HashSet<EstadoTurno> SensitiveStates = new()
    {
        EstadoTurno.Cancelado,
        EstadoTurno.NoShow
    };

    private static readonly Dictionary<EstadoTurno, HashSet<EstadoTurno>> AllowedStateTransitions = new()
    {
        [EstadoTurno.Pendiente] = new() { EstadoTurno.Confirmado, EstadoTurno.Atendido },
        [EstadoTurno.Confirmado] = new() { EstadoTurno.Atendido },
        [EstadoTurno.Atendido] = new(),
        [EstadoTurno.Cancelado] = new(),
        [EstadoTurno.NoShow] = new()
    };

    private readonly AppDbContext _context;
    private readonly INoShowPenaltyEvaluator _noShowPenaltyEvaluator;

    public TurnosController(AppDbContext context, INoShowPenaltyEvaluator noShowPenaltyEvaluator)
    {
        _context = context;
        _noShowPenaltyEvaluator = noShowPenaltyEvaluator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var turnos = await _context.Turnos
            .Include(t => t.Paciente)
            .Include(t => t.Medico)
            .ToListAsync();
        return Ok(turnos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var turno = await _context.Turnos
            .Include(t => t.Paciente)
            .Include(t => t.Medico)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (turno == null) return NotFound();
        return Ok(turno);
    }

    [HttpPost]
    public async Task<IActionResult> CrearTurno([FromBody] Turno turno)
    {
        var paciente = await _context.Pacientes.FindAsync(turno.PacienteId);
        if (paciente == null)
            return NotFound(new { mensaje = "Paciente no encontrado." });

        var noShowSettings = NoShowPenaltySettings.WithDefaults();
        var bloqueado = await TryAutoUnlockAndCheckActiveBlockAsync(paciente, noShowSettings);
        if (bloqueado)
            return BadRequest(new { mensaje = "El paciente se encuentra bloqueado para agendar turnos online." });

        var medicoExiste = await _context.Medicos.AnyAsync(m => m.Id == turno.MedicoId);
        if (!medicoExiste)
            return NotFound(new { mensaje = "Médico no encontrado." });

        var turnoConflicto = await _context.Turnos.AnyAsync(t =>
            t.MedicoId == turno.MedicoId &&
            t.FechaHora == turno.FechaHora &&
            t.Estado != EstadoTurno.Cancelado);
        if (turnoConflicto)
            return BadRequest(new { mensaje = "El médico ya tiene un turno en ese horario." });

        turno.FechaCreacion = DateTime.UtcNow;
        turno.Estado = EstadoTurno.Pendiente;
        turno.AusenciaPenalizada = false;
        _context.Turnos.Add(turno);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = turno.Id }, turno);
    }

    private async Task<bool> TryAutoUnlockAndCheckActiveBlockAsync(Paciente paciente, NoShowPenaltySettings settings)
    {
        if (!paciente.FechaBloqueo.HasValue)
            return false;

        var ahora = DateTime.Now;
        var bloqueoVence = paciente.FechaBloqueo.Value.AddDays(settings.BloqueoDias!.Value);
        if (bloqueoVence > ahora)
            return true;

        paciente.FechaBloqueo = null;
        await _context.SaveChangesAsync();
        return false;
    }

    [HttpPut("{id}/cancelar")]
    public async Task<IActionResult> CancelarTurno(int id)
    {
        var turno = await _context.Turnos.FindAsync(id);
        if (turno == null) return NotFound();

        if (turno.Estado == EstadoTurno.Cancelado)
            return BadRequest(new { mensaje = "El turno ya se encuentra cancelado." });

        if (turno.Estado == EstadoTurno.Atendido || turno.Estado == EstadoTurno.NoShow)
            return BadRequest(new { mensaje = "Solo se pueden cancelar turnos en estado Pendiente o Confirmado." });

        if (turno.FechaHora.IsExpired())
            return BadRequest(new { mensaje = "No se puede cancelar un turno ya vencido." });

        if (turno.FechaHora.IsWithinCancellationWindow())
        {
            turno.Estado = EstadoTurno.NoShow;
            turno.AusenciaPenalizada = false;
            await _context.SaveChangesAsync();
            await _noShowPenaltyEvaluator.EvaluateAndApplyAsync(turno.PacienteId);
            return Ok(new { mensaje = "Cancelación tardía: el turno fue marcado como ausencia.", turno });
        }

        turno.Estado = EstadoTurno.Cancelado;
        await _context.SaveChangesAsync();
        return Ok(turno);
    }

    [HttpPost("{id}/ausencia")]
    public async Task<IActionResult> MarcarAusencia(int id)
    {
        var turno = await _context.Turnos.FindAsync(id);
        if (turno == null) return NotFound();

        if (turno.Estado == EstadoTurno.NoShow)
            return BadRequest(new { mensaje = "El turno ya se encuentra marcado como ausencia." });

        if (turno.Estado == EstadoTurno.Atendido || turno.Estado == EstadoTurno.Cancelado)
            return BadRequest(new { mensaje = "Solo se puede marcar ausencia para turnos en estado Pendiente o Confirmado." });

        if (!turno.FechaHora.IsWithinCancellationWindow())
            return BadRequest(new { mensaje = "La ausencia solo puede registrarse dentro de las 24 horas del turno." });

        turno.Estado = EstadoTurno.NoShow;
        turno.AusenciaPenalizada = false;
        await _context.SaveChangesAsync();
        await _noShowPenaltyEvaluator.EvaluateAndApplyAsync(turno.PacienteId);
        return Ok(turno);
    }

    [HttpPut("{id}/estado")]
    public async Task<IActionResult> ActualizarEstado(int id, [FromBody] ActualizarEstadoRequest request)
    {
        var turno = await _context.Turnos.FindAsync(id);
        if (turno == null) return NotFound();

        if (turno.Estado == request.Estado)
            return BadRequest(new { mensaje = "El turno ya se encuentra en el estado solicitado." });

        if (SensitiveStates.Contains(request.Estado))
            return BadRequest(new { mensaje = "No se permite actualizar a ese estado por este endpoint. Use los endpoints de negocio específicos." });

        if (!AllowedStateTransitions.TryGetValue(turno.Estado, out var allowedStates) ||
            !allowedStates.Contains(request.Estado))
            return BadRequest(new { mensaje = $"Transición inválida desde {turno.Estado} hacia {request.Estado}." });

        turno.Estado = request.Estado;
        await _context.SaveChangesAsync();
        return Ok(turno);
    }
}

public class ActualizarEstadoRequest
{
    public EstadoTurno Estado { get; set; }
}
