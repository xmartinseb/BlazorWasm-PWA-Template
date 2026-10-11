using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

namespace MojePwa.Client.Services.DataServices;

public abstract class ServiceBase(HttpClient httpClient)
{
    const string GenericError = "Operaci se nepodařilo dokončit kvůli neočekávané chybě";
    const string ConnectionError = "Nepodařilo se spojit se serverem";
    const string InvalidJsonError = "Chyba formátu JSON";

    // TODO: zpracovani http requestu do samostatneho parametru
    protected Task<Result<string>> GetStringAsync(string url, CT ct)
        => HttpOperationAsync(async () =>
        {
            using var response = await httpClient.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode)
                return Result.Err<string>(await GetHttpErrorAsync(response, ct));

            string resultString = await response.Content.ReadAsStringAsync(ct);
            return resultString is null
                ? Result.Err<string>("Server vrátil prázdnou textovou odpověď.")
                : Result.Ok(resultString);
        }, Result.Err<string>);

    protected Task<Result<T>> GetFromJsonAsync<T>(string url, CT ct)
        => HttpOperationAsync(async () =>
        {
            using var response = await httpClient.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode)
                return Result.Err<T>(await GetHttpErrorAsync(response, ct));

            var resultObj = await response.Content.ReadFromJsonAsync<T>(ct);
            return resultObj is null
                ? Result.Err<T>("Server vrátil prázdnou odpověď.")
                : Result.Ok(resultObj);
        }, Result.Err<T>);

    protected Task<Result> PostAsJsonAsync<T>(string url, T data, CT ct)
        => HttpOperationAsync(async () =>
        {
            using var result = await httpClient.PostAsJsonAsync(url, data, ct);
            if (!result.IsSuccessStatusCode)
                return Result.Err(await GetHttpErrorAsync(result, ct));
                
            return Result.Ok();
        }, Result.Err);

    static async Task<TResult> HttpOperationAsync<TResult>(
        Func<Task<TResult>> action, Func<IReadOnlyList<string>, TResult> createError) where TResult : Result
    {
        try
        {
            return await action();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            return createError([GetHttpErrorMessage(ex)]);
        }
        catch (JsonException)
        {
            return createError([InvalidJsonError]);
        }
        catch (Exception)
        {
            return createError([GenericError]);
        }
    }

    static string GetHttpErrorMessage(HttpRequestException exception)
        => exception.StatusCode is { } statusCode
            ? $"Požadavek na serveru selhal (HTTP {(int)statusCode} {statusCode})."
            : ConnectionError;

    static async Task<string> GetHttpErrorAsync(HttpResponseMessage response, CT ct)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>(ct) is { Detail: { } problemDetail }
                ? problemDetail
                : GenericError;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return GenericError;
        }
    }

    /// <summary>
    /// Podmnožina properties, které se vyčítají ze standardních chybového formátu RFC 9475
    /// </summary>
    sealed record ProblemDetailsResponse(string Detail);

    /// <summary>Příkaz vracející <see cref="Result"/>.</summary>
    protected static Task<Result> RunAsync(Func<ServiceOperationContext, Task<Result>> action)
        => ExecuteAsync(action, Result.Err);

    /// <summary>Příkaz vracející <see cref="Result{T}"/>.</summary>
    protected static Task<Result<T>> RunAsync<T>(Func<ServiceOperationContext, Task<Result<T>>> action)
        => ExecuteAsync(action, Result.Err<T>);

    static async Task<TResult> ExecuteAsync<TResult>(
        Func<ServiceOperationContext, Task<TResult>> action, Func<IReadOnlyList<string>, TResult> error) where TResult : Result
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
            Debug.WriteLine("Unexpected client error: {0}", ex);
            return error([GenericError]);
        }
    }

    /// <summary>
    /// Místo pro data společná všem service requestům, např. info o přihlášeném uživateli apod.
    /// </summary>
    public readonly record struct ServiceOperationContext();
}