namespace TurnosMedicos.Services;

public class NoShowPenaltySettings
{
    public string? MedidaPeriodo { get; set; }
    public int? Periodo { get; set; }
    public int? BloqueoDias { get; set; }

    public static NoShowPenaltySettings WithDefaults(NoShowPenaltySettings? input = null)
    {
        input ??= new NoShowPenaltySettings();

        var medida = string.Equals(input.MedidaPeriodo, "Dia", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(input.MedidaPeriodo, "Mes", StringComparison.OrdinalIgnoreCase)
            ? input.MedidaPeriodo!
            : "Mes";

        var periodo = input.Periodo.HasValue && input.Periodo.Value >= 0
            ? input.Periodo.Value
            : 1;

        var bloqueoDias = input.BloqueoDias.HasValue && input.BloqueoDias.Value >= 0
            ? input.BloqueoDias.Value
            : 30;

        return new NoShowPenaltySettings
        {
            MedidaPeriodo = medida,
            Periodo = periodo,
            BloqueoDias = bloqueoDias
        };
    }
}
