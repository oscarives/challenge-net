namespace TurnosMedicos.Services;

public interface INoShowPenaltyEvaluator
{
    Task EvaluateAndApplyAsync(int? pacienteId);
}
