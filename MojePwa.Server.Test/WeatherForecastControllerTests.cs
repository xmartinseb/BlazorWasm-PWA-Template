using MojePwa.Server.Controllers;
using Xunit;

namespace MojePwa.Server.Test;

public sealed class WeatherForecastControllerTests
{
    [Fact]
    public void GetForecasts_ReturnsFiveConsecutiveForecasts()
    {
        var controller = new WeatherForecastController();

        var forecasts = controller.GetForecasts().ToArray();

        Assert.Equal(5, forecasts.Length);
        Assert.All(forecasts, forecast =>
        {
            Assert.InRange(forecast.TemperatureC, -20, 54);
            Assert.False(string.IsNullOrWhiteSpace(forecast.Summary));
        });

        for (var index = 1; index < forecasts.Length; index++)
        {
            Assert.Equal(forecasts[index - 1].Date.AddDays(1), forecasts[index].Date);
        }
    }
}
