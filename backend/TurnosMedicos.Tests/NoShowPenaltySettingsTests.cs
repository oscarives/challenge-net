using TurnosMedicos.Services;

namespace TurnosMedicos.Tests;

public class NoShowPenaltySettingsTests
{
    [Fact]
    public void WithDefaults_UsesFallbacks_WhenValuesAreInvalid()
    {
        var settings = NoShowPenaltySettings.WithDefaults(new NoShowPenaltySettings
        {
            MedidaPeriodo = "Invalido",
            Periodo = -1,
            BloqueoDias = -10
        });

        Assert.Equal("Mes", settings.MedidaPeriodo);
        Assert.Equal(1, settings.Periodo);
        Assert.Equal(30, settings.BloqueoDias);
    }
}
