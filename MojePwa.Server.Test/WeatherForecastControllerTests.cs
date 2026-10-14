using MojePwa.Domain;
using System.Net.Http.Json;
using Xunit;

namespace MojePwa.Server.Test;

public sealed class WeatherForecastControllerTests : ControllerTestsBase
{
    protected override string ControllerUrl => "/api/weather-forecast";

    [Fact]
    public Task GetForecasts_ReturnsFiveConsecutiveForecasts()
        => DoAsync(async client =>
    {
        var forecasts = await client.GetFromJsonAsync<WeatherForecast[]>(ControllerUrl);
    });
}
