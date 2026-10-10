using System.Net.Http.Json;
using System.Text.Json;

namespace MojePwa.Client.Services.DataServices;

public abstract class ServiceBase(HttpClient httpClient)
{
    const string GenericError = "Operaci se nepodařilo dokončit kvůli neočekávané chybě.";

    HttpClient HttpClient { get; } = httpClient;

    protected async Task<Result<T>> GetFromJsonAsync<T>(string url, CT ct)
    {
        try
        {
            var resultObj = await HttpClient.GetFromJsonAsync<T>(url, ct);
            return Result.Ok(resultObj!);
        }
        catch (HttpRequestException ex)
        {
            // TODO: je tam standardni popis chyby. Do notifikace title a message
        }
        catch (JsonException ex)
        {
            // TODO: do notifikace chyba jsonu
        }
        catch (Exception ex)
        {

        }
    }


    protected async Task<Result> PostAsJsonAsync<T>(string url, T data, CT ct)
    {
        try
        {
            var result = await HttpClient.PostAsJsonAsync(url, data, ct);
            if (result.IsSuccessStatusCode)
                return Result.Ok();
        }
        catch (HttpRequestException ex)
        {
            // TODO: je tam standardni popis chyby. Do notifikace title a message
        }
        catch (Exception ex)
        {
            // TODO: do notifikace obecna chyba
        }
    }

    /// <summary>Příkaz vracející <see cref="Result"/>.</summary>
    protected Task<Result> RunAsync(CT ct, Func<ServiceOperationContext, Task<Result>> action)
        => ExecuteAsync(ct, action, Result.Err);

    /// <summary>Příkaz vracející <see cref="Result{T}"/>.</summary>
    protected Task<Result<T>> RunAsync<T>(CT ct, Func<ServiceOperationContext, Task<Result<T>>> action)
        => ExecuteAsync(ct, action, Result.Err<T>);

    async Task<TResult> ExecuteAsync<TResult>(
        CT ct,
        Func<ServiceOperationContext, Task<TResult>> action,
        Func<IReadOnlyList<string>, TResult> error) where TResult : Result
    {
        try
        {
            return await action(new ServiceOperationContext());
        }
        catch (OperationCanceledException)
        {
            throw; // zrušení přes ct není chyba – propadne dál
        }
        catch (UserFriendlyServiceFailException ex)
        {
            return error(ex.Errors);
        }
        catch (Exception ex)
        {
            //logger.LogError(ex, "Neočekávaná chyba");
            return error([GenericError]);
        }
    }

    /// <summary>
    /// Místo pro data společná všem service requestům, např. info o přihlášeném uživateli apod.
    /// </summary>
    public readonly record struct ServiceOperationContext();
}