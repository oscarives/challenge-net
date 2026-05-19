using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TurnosMedicos.Controllers;
using TurnosMedicos.Data;
using TurnosMedicos.Models;
using TurnosMedicos.Services;

namespace TurnosMedicos.Tests;

public class PacientesControllerTests
{
    [Fact]
    public async Task GetById_ReturnsBloqueadoVigenteAndBloqueoHastaUtc_FromServerCalculation()
    {
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        await using var context = new AppDbContext(options);
        context.Pacientes.Add(new Paciente
        {
            Id = 9001,
            NombreCompleto = "Paciente Bloqueado",
            DNI = "123",
            Email = "p@x.com",
            Telefono = "111",
            FechaBloqueo = DateTime.UtcNow.AddDays(-1),
            createdAt = DateTime.UtcNow,
            isActive = true
        });
        await context.SaveChangesAsync();

        var controller = new PacientesController(
            context,
            Options.Create(new NoShowPenaltySettings { BloqueoDias = 30, MedidaPeriodo = "Mes", Periodo = 1 }));

        var result = await controller.GetById(9001);
        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<PacienteResponse>(ok.Value);

        Assert.True(payload.BloqueadoVigente);
        Assert.NotNull(payload.BloqueoHastaUtc);
    }
}
