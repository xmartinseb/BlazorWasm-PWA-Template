using System.Net.Http.Json;

namespace MojePwa.Client.Services.DataServices;

public interface IFakeDataService
{
    Task<Result<Dictionary<string, string>>> GetAllDataAsync(CT ct);
    Task<Result> AddFakeDataAsync(string key, string value, CT ct);
}

public sealed class FakeDataService(HttpClient httpClient)
    : ServiceBase(httpClient), IFakeDataService
{
    public Task<Result> AddFakeDataAsync(string key, string value, CT ct)
    => RunAsync(ct, async ctx =>
    {
        await Task.Delay(1500, ct); // Fake delay (GUI test)
        using var response = await HttpClient.PostAsJsonAsync("api/FakeData", new KeyValuePair<string, string>(key, value), ct);
        if (!response.IsSuccessStatusCode)
            return Result.Err($"Uložení na serveru selhalo (HTTP {(int)response.StatusCode}).");

        return Result.Ok();
    });

    public Task<Result<Dictionary<string, string>>> GetAllDataAsync(CT ct)
    => RunAsync(ct, async ctx =>
    {
        await Task.Delay(1500, ct); // Fake delay (GUI test)
        var fakeData = await HttpClient.GetFromJsonAsync<Dictionary<string, string>>("api/FakeData", ct);
        return Result.Ok(fakeData ?? []);
    });
}