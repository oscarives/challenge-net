using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TurnosMedicos.Data;
using TurnosMedicos.Models;

namespace TurnosMedicos.Services;

public class NoShowPenaltyEvaluator : INoShowPenaltyEvaluator
{
    private readonly AppDbContext _context;
    private readonly NoShowPenaltySettings _settings;

    public NoShowPenaltyEvaluator(AppDbContext context, IOptions<NoShowPenaltySettings> settings)
    {
        _context = context;
        _settings = NoShowPenaltySettings.WithDefaults(settings.Value);
    }

    public async Task EvaluateAndApplyAsync(int? pacienteId)
    {
        if (!pacienteId.HasValue)
            return;

        var paciente = await _context.Pacientes.FindAsync(pacienteId.Value);
        if (paciente == null)
            return;

        var now = DateTime.Now;
        var start = string.Equals(_settings.MedidaPeriodo, "Dia", StringComparison.OrdinalIgnoreCase)
            ? now.AddDays(-_settings.Periodo!.Value)
            : now.AddMonths(-_settings.Periodo!.Value);

        var noShowsNoPenalizados = await _context.Turnos
            .Where(t => t.PacienteId == pacienteId.Value &&
                        t.Estado == EstadoTurno.NoShow &&
                        !t.AusenciaPenalizada &&
                        t.FechaHora >= start)
            .OrderBy(t => t.FechaHora)
            .ToListAsync();

        if (noShowsNoPenalizados.Count < 3)
            return;

        foreach (var turno in noShowsNoPenalizados)
            turno.AusenciaPenalizada = true;

        var bloqueoVigente = paciente.FechaBloqueo.HasValue &&
                             paciente.FechaBloqueo.Value.AddDays(_settings.BloqueoDias!.Value) > now;

        if (!bloqueoVigente)
            paciente.FechaBloqueo = now;

        paciente.Bloqueado = true;

        await _context.SaveChangesAsync();
    }
}
