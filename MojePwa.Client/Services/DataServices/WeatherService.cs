using MojePwa.Domain;

namespace MojePwa.Client.Services.DataServices;

public interface IWeatherService
{
    Task<Result<WeatherForecast[]>> GetForecastAsync(CT ct);
}

public sealed class WeatherService(HttpClient httpClient)
    : ServiceBase(httpClient), IWeatherService
{

    public Task<Result<WeatherForecast[]>> GetForecastAsync(CT ct)
    => RunAsync(async context =>
    {
        await Task.Delay(800, ct); // Fake delay (GUI test)
        return await GetFromJsonAsync<WeatherForecast[]>("api/weather-forecast", ct);
    });
}