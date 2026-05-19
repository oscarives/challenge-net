namespace TurnosMedicos.Services;

public class NoOpNoShowPenaltyEvaluator : INoShowPenaltyEvaluator
{
    public Task EvaluateAndApplyAsync(int? pacienteId)
    {
        return Task.CompletedTask;
    }
}
