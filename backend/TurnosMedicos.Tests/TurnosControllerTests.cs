using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TurnosMedicos.Controllers;
using TurnosMedicos.Data;
using TurnosMedicos.Models;
using TurnosMedicos.Services;

namespace TurnosMedicos.Tests;

public class TurnosControllerTests
{
    [Fact]
    public async Task ActualizarEstado_AllowsValidTransition_AndBlocksInvalidAndSensitive()
    {
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        await using var context = new AppDbContext(options);
        context.Turnos.Add(new Turno
        {
            Id = 3001,
            PacienteId = 1,
            MedicoId = 1,
            FechaHora = DateTime.Now.AddDays(1),
            Estado = EstadoTurno.Pendiente,
            FechaCreacion = DateTime.UtcNow,
            Motivo = "test"
        });
        await context.SaveChangesAsync();

        var controller = new TurnosController(
            context,
            new NoOpNoShowPenaltyEvaluator(),
            Options.Create(new NoShowPenaltySettings { MedidaPeriodo = "Mes", Periodo = 1, BloqueoDias = 30 }));

        var ok = await controller.ActualizarEstado(3001, new ActualizarEstadoRequest { Estado = EstadoTurno.Confirmado });
        Assert.IsType<OkObjectResult>(ok);

        var invalid = await controller.ActualizarEstado(3001, new ActualizarEstadoRequest { Estado = EstadoTurno.Pendiente });
        Assert.IsType<BadRequestObjectResult>(invalid);

        var sensitive = await controller.ActualizarEstado(3001, new ActualizarEstadoRequest { Estado = EstadoTurno.Cancelado });
        Assert.IsType<BadRequestObjectResult>(sensitive);

        var sensitiveAtendido = await controller.ActualizarEstado(3001, new ActualizarEstadoRequest { Estado = EstadoTurno.Atendido });
        var badSensitiveAtendido = Assert.IsType<BadRequestObjectResult>(sensitiveAtendido);
        Assert.Contains("No se permite actualizar a ese estado", GetMensaje(badSensitiveAtendido.Value));
    }

    [Fact]
    public async Task CrearTurno_RespectsBloqueoDiasOverride()
    {
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        await using var context = new AppDbContext(options);
        context.Medicos.Add(new Medico { Id = 4001, NombreCompleto = "Dr Test", Especialidad = "Clinica", SucursalId = 1 });
        context.Pacientes.Add(new Paciente
        {
            Id = 4002,
            NombreCompleto = "Paciente Bloqueado",
            DNI = "999",
            Email = "b@b.com",
            Telefono = "222",
            FechaBloqueo = DateTime.Now,
            createdAt = DateTime.UtcNow,
            isActive = true
        });
        await context.SaveChangesAsync();

        var controller = new TurnosController(
            context,
            new NoOpNoShowPenaltyEvaluator(),
            Options.Create(new NoShowPenaltySettings { MedidaPeriodo = "Mes", Periodo = 1, BloqueoDias = 0 }));

        var result = await controller.CrearTurno(new Turno
        {
            PacienteId = 4002,
            MedicoId = 4001,
            FechaHora = DateTime.Now.AddDays(2),
            Motivo = "test"
        });

        Assert.IsType<CreatedAtActionResult>(result);

        var paciente = await context.Pacientes.FindAsync(4002);
        Assert.NotNull(paciente);
        Assert.Null(paciente!.FechaBloqueo);
    }

    [Fact]
    public async Task CancelarTurno_ReturnsBadRequest_WhenTurnoExpired()
    {
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        await using var context = new AppDbContext(options);
        context.Turnos.Add(new Turno
        {
            Id = 5001,
            PacienteId = 1,
            MedicoId = 1,
            FechaHora = DateTime.UtcNow.AddHours(-1),
            Estado = EstadoTurno.Confirmado,
            FechaCreacion = DateTime.UtcNow,
            Motivo = "test"
        });
        await context.SaveChangesAsync();

        var controller = new TurnosController(
            context,
            new NoOpNoShowPenaltyEvaluator(),
            Options.Create(new NoShowPenaltySettings { MedidaPeriodo = "Mes", Periodo = 1, BloqueoDias = 30 }));

        var result = await controller.CancelarTurno(5001);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("turno ya vencido", GetMensaje(badRequest.Value));
    }

    [Fact]
    public async Task MarcarAusencia_ReturnsBadRequest_WhenOutsideWindow()
    {
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        await using var context = new AppDbContext(options);
        context.Turnos.Add(new Turno
        {
            Id = 5002,
            PacienteId = 1,
            MedicoId = 1,
            FechaHora = DateTime.UtcNow.AddHours(25),
            Estado = EstadoTurno.Pendiente,
            FechaCreacion = DateTime.UtcNow,
            Motivo = "test"
        });
        await context.SaveChangesAsync();

        var controller = new TurnosController(
            context,
            new NoOpNoShowPenaltyEvaluator(),
            Options.Create(new NoShowPenaltySettings { MedidaPeriodo = "Mes", Periodo = 1, BloqueoDias = 30 }));

        var result = await controller.MarcarAusencia(5002);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("24 horas", GetMensaje(badRequest.Value));
    }

    private static string GetMensaje(object? value)
    {
        if (value == null)
            return string.Empty;

        var type = value.GetType();
        var mensaje = type.GetProperty("mensaje")?.GetValue(value)?.ToString();
        if (!string.IsNullOrWhiteSpace(mensaje))
            return mensaje;

        var message = type.GetProperty("message")?.GetValue(value)?.ToString();
        return message ?? string.Empty;
    }
}
