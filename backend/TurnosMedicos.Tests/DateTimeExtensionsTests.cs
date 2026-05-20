using TurnosMedicos.Helpers;

namespace TurnosMedicos.Tests;

public class DateTimeExtensionsTests
{
    [Fact]
    public void IsWithinCancellationWindow_ReturnsTrue_ForExactlyTwentyFourHours()
    {
        var fecha = DateTime.Now.AddHours(24);
        Assert.True(fecha.IsWithinCancellationWindow());
    }

    [Fact]
    public void IsWithinCancellationWindow_ReturnsFalse_ForMoreThanTwentyFourHours()
    {
        var fecha = DateTime.Now.AddHours(25);
        Assert.False(fecha.IsWithinCancellationWindow());
    }

    [Fact]
    public void IsWithinCancellationWindow_ReturnsFalse_AndIsExpiredTrue_ForPastTurno()
    {
        var fecha = DateTime.Now.AddHours(-1);
        Assert.False(fecha.IsWithinCancellationWindow());
        Assert.True(fecha.IsExpired());
    }
}
