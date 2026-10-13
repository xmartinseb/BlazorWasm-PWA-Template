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
    => RunAsync(async context =>
    {
        await Task.Delay(1500, ct); // Fake delay (GUI test)
        return await PutAsJsonAsync("api/fake-data/set-key", new KeyValuePair<string, string>(key, value), ct);
    });

    public Task<Result<Dictionary<string, string>>> GetAllDataAsync(CT ct)
    => RunAsync(async context =>
    {
        await Task.Delay(1500, ct); // Fake delay (GUI test)
        return await GetFromJsonAsync<Dictionary<string, string>>("api/fake-data", ct);
    });
}