using MojePwa.Domain;
using System.Net.Http.Json;

namespace MojePwa.Client.Services.DataServices;

public interface IWeatherService
{
    Task<Result<WeatherForecast[]>> GetForecastAsync(CT ct);
}

public sealed class WeatherService(HttpClient httpClient)
    : ServiceBase(httpClient), IWeatherService
{

    public Task<Result<WeatherForecast[]>> GetForecastAsync(CT ct)
    => RunAsync(ct, async ctx =>
    {
        var forecasts = await HttpClient.GetFromJsonAsync<WeatherForecast[]>("WeatherForecast", ct);
        return Result.Ok(forecasts ?? []);
    });
}