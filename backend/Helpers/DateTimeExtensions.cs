namespace TurnosMedicos.Helpers;

public static class DateTimeExtensions
{
    public static double HoursUntil(this DateTime fechaTurno)
    {
        return (fechaTurno - DateTime.Now).TotalHours;
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
