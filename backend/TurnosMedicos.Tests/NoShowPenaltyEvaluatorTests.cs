using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TurnosMedicos.Data;
using TurnosMedicos.Models;
using TurnosMedicos.Services;

namespace TurnosMedicos.Tests;

public class NoShowPenaltyEvaluatorTests
{
    [Fact]
    public async Task EvaluateAndApplyAsync_BlocksAndMarksPenalized_WhenThreeNoShows()
    {
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        await using var context = new AppDbContext(options);
        context.Pacientes.Add(new Paciente
        {
            Id = 1001,
            NombreCompleto = "Paciente Test",
            DNI = "123",
            Email = "a@a.com",
            Telefono = "111",
            createdAt = DateTime.UtcNow,
            isActive = true
        });

        var now = DateTime.Now;
        context.Turnos.AddRange(
            new Turno { Id = 2001, PacienteId = 1001, MedicoId = 1, FechaHora = now.AddHours(-1), Estado = EstadoTurno.NoShow, FechaCreacion = DateTime.UtcNow, Motivo = "m1", AusenciaPenalizada = false },
            new Turno { Id = 2002, PacienteId = 1001, MedicoId = 2, FechaHora = now.AddHours(-2), Estado = EstadoTurno.NoShow, FechaCreacion = DateTime.UtcNow, Motivo = "m2", AusenciaPenalizada = false },
            new Turno { Id = 2003, PacienteId = 1001, MedicoId = 3, FechaHora = now.AddHours(-3), Estado = EstadoTurno.NoShow, FechaCreacion = DateTime.UtcNow, Motivo = "m3", AusenciaPenalizada = false }
        );
        await context.SaveChangesAsync();

        var settings = Options.Create(new NoShowPenaltySettings { MedidaPeriodo = "Mes", Periodo = 1, BloqueoDias = 30 });
        var evaluator = new NoShowPenaltyEvaluator(context, settings);

        await evaluator.EvaluateAndApplyAsync(1001);

        var paciente = await context.Pacientes.FindAsync(1001);
        Assert.NotNull(paciente);
        Assert.NotNull(paciente!.FechaBloqueo);

        var penalizadas = await context.Turnos.Where(t => t.PacienteId == 1001).AllAsync(t => t.AusenciaPenalizada);
        Assert.True(penalizadas);
    }
}
