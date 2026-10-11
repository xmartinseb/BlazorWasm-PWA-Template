namespace MojePwa.Client.Services.DataServices;

public interface ITestApiService
{
    Task<Result<string>> TestApiGetAsync(CT ct);
}
public sealed class TestApiService(HttpClient httpClient) : ServiceBase(httpClient), ITestApiService
{
    public Task<Result<string>> TestApiGetAsync(CT ct)
        => RunAsync(ct, async ctx =>
        {
            return await GetStringAsync("api/test", ct);
        });
}