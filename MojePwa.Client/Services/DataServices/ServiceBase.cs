using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

namespace MojePwa.Client.Services.DataServices;

/// <summary>
/// Poskytuje pro servicy HTTP operace s jednotným chováním a zpracováním chyb
/// </summary>
public abstract class ServiceBase(HttpClient httpClient)
{
    const string GenericError = "Operaci se nepodařilo dokončit kvůli neočekávané chybě";
    const string ConnectionError = "Nepodařilo se spojit se serverem";
    const string InvalidJsonError = "Chyba formátu JSON";


    /// <summary>
    /// HTTP Get operace; vrácená data se načtou jako obyčejný string.
    /// Obyčejný string se nedá parsovat jako JSON objekt!
    /// </summary>
    protected Task<Result<string>> GetStringAsync(string url, CT ct)
        => HttpOperationAsync(
            () => httpClient.GetAsync(url, ct),
            async response => await response.Content.ReadAsStringAsync(ct) is string responseBodyString
                ? Result.Ok(responseBodyString)
                : Result.Err<string>("Server vrátil prázdnou textovou odpověď."),
            Result.Err<string>, ct);


    /// <summary>
    /// HTTP Get operace s načtením dat z jsonu
    /// </summary>
    /// <typeparam name="TResponse">Typ načítaných dat</typeparam>
    protected Task<Result<TResponse>> GetFromJsonAsync<TResponse>(string url, CT ct)
        => HttpOperationAsync(
            () => httpClient.GetAsync(url, ct),
            async response => await response.Content.ReadFromJsonAsync<TResponse>(ct) is { } resultObj
                    ? Result.Ok(resultObj)
                    : Result.Err<TResponse>("Server vrátil prázdnou odpověď."), Result.Err<TResponse>, ct);

    /// <summary>
    /// HTTP Post operace bez čtení dat z response
    /// </summary>
    /// <typeparam name="TUpload">Typ nahrávaných dat</typeparam>
    protected Task<Result> PostAsJsonAsync<TUpload>(string url, TUpload data, CT ct)
        => HttpOperationAsync(
            () => httpClient.PostAsJsonAsync(url, data, ct), 
            async response => Result.Ok(),
            Result.Err, ct);

    /// <summary>
    /// HTTP Post operace bez čtení dat z response
    /// </summary>
    /// <typeparam name="TUpload">Typ nahrávaných dat</typeparam>
    protected Task<Result> PutAsJsonAsync<TUpload>(string url, TUpload data, CT ct)
        => HttpOperationAsync(
            () => httpClient.PutAsJsonAsync(url, data, ct),
            async response => Result.Ok(),
            Result.Err, ct);

    /// <summary>
    /// HTTP Post operace s načtením dat z response
    /// </summary>
    /// <typeparam name="TUpload">Typ nahrávaných dat</typeparam>
    /// <typeparam name="TResponse">Typ načtených dat z response</typeparam>
    protected Task<Result<TResponse>> PostAsJsonAsync<TUpload, TResponse>(string url, TUpload data, CT ct)
        => HttpOperationAsync(
            () => httpClient.PostAsJsonAsync(url, data, ct),
            async response => await response.Content.ReadFromJsonAsync<TResponse>(ct) is { } resultObj
                    ? Result.Ok(resultObj)
                    : Result.Err<TResponse>("Server vrátil prázdnou odpověď."), Result.Err<TResponse>, ct);

    /// <summary>
    /// Jednotné zpracování chyb jedním try/catch pro všechny HTTP požadavky.
    /// </summary>
    /// <typeparam name="THttpResult">Typ resultu: buďto Result bez dat, nebo Result s T value</typeparam>
    /// <param name="sendHttpRequest">Operace odeslání http požadavku. Má jen vrátit response</param>
    /// <param name="processResponse">Vyčtení dat z response (případně jen vrácení OK)</param>
    /// <param name="createError">Metoda pro vytvoření error resultu pro případ chyby</param>
    /// <returns>Result operace daného typu: Result bez dat, nebo Result s T value</returns>
    static async Task<THttpResult> HttpOperationAsync<THttpResult>(
        Func<Task<HttpResponseMessage>> sendHttpRequest,
        Func<HttpResponseMessage, Task<THttpResult>> processResponse,
        Func<IReadOnlyList<string>, THttpResult> createError,
        CT ct) where THttpResult : Result
    {
        try
        {
            using var response = await sendHttpRequest();
            if (!response.IsSuccessStatusCode)
                return createError([await GetHttpErrorAsync(response, ct)]);

            return await processResponse(response);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw; // skutečné zrušení requestu
        }
        catch (OperationCanceledException)
        {
            return createError(["Server neodpověděl včas"]); // timeout, neni to zrušení
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
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>(ct);

            return problemDetails?.Detail
                ?? problemDetails?.Errors?.Values.SelectMany(v => v).FirstOrDefault()
                ?? problemDetails?.Title
                ?? GenericError;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return "Chyba zpracování chyb na straně klienta";
        }
    }

    /// <summary>
    /// Podmnožina properties, které se vyčítají ze standardních chybového formátu RFC 9475
    /// </summary>
    /// <param name="Title">Nadpis chyby, součást JSON formátu RFC 9475</param>
    /// <param name="Detail">Popis chyby, součást JSON formátu RFC 9475</param>
    /// <param name="Errors">Chyby validace akcí</param>
    sealed record ProblemDetailsResponse(string? Title, string? Detail, Dictionary<string, string[]>? Errors); // TODO: Vyzkoušet Errors

    /// <summary>
    /// Obálka pro vykonání kódu v metodě servicy, která dodává jednotný kontext a jednotné zpracování případných exceptions
    /// </summary>
    protected static Task<Result> RunAsync(Func<ServiceOperationContext, Task<Result>> action)
        => ExecuteAsync(action, Result.Err);

    /// <summary>
    /// Obálka pro vykonání kódu v metodě servicy, která dodává jednotný kontext a jednotné zpracování případných exceptions
    /// </summary>
    protected static Task<Result<T>> RunAsync<T>(Func<ServiceOperationContext, Task<Result<T>>> action)
        => ExecuteAsync(action, Result.Err<T>);

    /// <summary>
    /// Jednotný základ pro varianty s Result bez dat a Result s T value
    /// </summary>
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
    /// Zatím prázdná, patrně bude potřeba později, až se aplikace rozvine.
    /// </summary>
    public readonly record struct ServiceOperationContext();
}