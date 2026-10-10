using System.Net.Http.Json;
using System.Text.Json;

namespace MojePwa.Client.Services.DataServices;

public abstract class ServiceBase(HttpClient httpClient)
{
    const string GenericError = "Operaci se nepodařilo dokončit kvůli neočekávané chybě.";
    const string ConnectionError = "Nepodařilo se spojit se serverem.";
    const string InvalidJsonError = "Server vrátil neplatná data.";

    HttpClient HttpClient { get; } = httpClient;

    protected async Task<Result<T>> GetFromJsonAsync<T>(string url, CT ct)
    {
        try
        {
            var resultObj = await HttpClient.GetFromJsonAsync<T>(url, ct);
            return resultObj is null
                ? Result.Err<T>("Server vrátil prázdnou odpověď.")
                : Result.Ok(resultObj);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            return Result.Err<T>(GetHttpError(ex));
        }
        catch (JsonException)
        {
            return Result.Err<T>(InvalidJsonError);
        }
        catch (Exception)
        {
            return Result.Err<T>(GenericError);
        }
    }


    protected async Task<Result> PostAsJsonAsync<T>(string url, T data, CT ct)
    {
        try
        {
            using var result = await HttpClient.PostAsJsonAsync(url, data, ct);
            if (result.IsSuccessStatusCode)
                return Result.Ok();

            return Result.Err(GetHttpError(result.StatusCode));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            return Result.Err(GetHttpError(ex));
        }
        catch (JsonException)
        {
            return Result.Err("Data se nepodařilo připravit k odeslání.");
        }
        catch (Exception)
        {
            return Result.Err(GenericError);
        }
    }

    static string GetHttpError(HttpRequestException exception)
        => exception.StatusCode is { } statusCode
            ? GetHttpError(statusCode)
            : ConnectionError;

    static string GetHttpError(System.Net.HttpStatusCode statusCode)
        => $"Požadavek na serveru selhal (HTTP {(int)statusCode} {statusCode}).";

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