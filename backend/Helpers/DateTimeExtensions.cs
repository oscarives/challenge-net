namespace TurnosMedicos.Helpers;

public static class DateTimeExtensions
{
    public static DateTime ToUtcNormalized(this DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Local).ToUniversalTime()
        };
    }

    public static double HoursUntil(this DateTime fechaTurno)
    {
        var turnoUtc = fechaTurno.ToUtcNormalized();
        return (turnoUtc - DateTime.UtcNow).TotalHours;
    }

    public static bool IsWithinCancellationWindow(this DateTime fechaTurno)
    {
        var hoursUntil = fechaTurno.HoursUntil();
        return hoursUntil >= 0 && hoursUntil <= 24;
    }

    public static bool IsExpired(this DateTime fechaTurno)
    {
        return fechaTurno.HoursUntil() < 0;
    }
}
