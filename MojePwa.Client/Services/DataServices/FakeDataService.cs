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
        return await PostAsJsonAsync("api/fake-data", new KeyValuePair<string, string>(key, value), ct);
    });

    public Task<Result<Dictionary<string, string>>> GetAllDataAsync(CT ct)
    => RunAsync(ct, async ctx =>
    {
        await Task.Delay(1500, ct); // Fake delay (GUI test)
        return await GetFromJsonAsync<Dictionary<string, string>>("api/fake-data", ct);
    });
}