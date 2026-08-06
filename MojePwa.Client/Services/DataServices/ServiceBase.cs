namespace MojePwa.Client.Services.DataServices;

public abstract class ServiceBase(HttpClient httpClient)
{
    const string GenericError = "Operaci se nepodařilo dokončit kvůli neočekávané chybě.";

    protected HttpClient HttpClient { get; } = httpClient;

    /// <summary>Příkaz vracející <see cref="Result"/>.</summary>
    protected Task<Result> RunAsync(CT ct, Func<ServiceOperationContext, Task<Result>> action)
        => ExecuteAsync(ct, action, Result.Err);

    /// <summary>Příkaz vracející <see cref="Result{T}"/>.</summary>
    protected Task<Result<T>> RunAsync<T>(CT ct, Func<ServiceOperationContext, Task<Result<T>>> action)
        => ExecuteAsync(ct, action, Result.Err<T>);

    /// <summary>
    /// Sdílené jádro: vytvoří krátkožijící DbContext, ověří přihlášeného uživatele a spustí akci.
    /// Typ výsledku i tvorbu chyby dodává volající přes <paramref name="error"/>, takže netřeba
    /// duplikovat boilerplate mezi <see cref="Result"/> a <see cref="Result{T}"/> variantou.
    /// </summary>
    async Task<TResult> ExecuteAsync<TResult>(
        CT ct,
        Func<ServiceOperationContext, Task<TResult>> action,
        Func<IReadOnlyList<string>, TResult> error) where TResult : Result
    {
        try
        {
            //await using var db = await dbFactory.CreateDbContextAsync(ct);
            //var currentUser = await currentUserAcc.GetUserInfoAsync();

            //if (currentUser is null)
            //    return error(["Není přihlášený žádný uživatel"]);

            //if (requiredRoles is { RequiredRoles: [_, ..] required } && !currentUser.Value.HasAnyRoleFrom(required))
            //    return error(["Nemáte oprávnění pro tuto operaci"]);

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

    public readonly record struct ServiceOperationContext();
}