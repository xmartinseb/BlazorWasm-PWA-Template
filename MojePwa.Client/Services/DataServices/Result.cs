namespace MojePwa.Client.Services.DataServices;

/// <summary>
/// Výsledek operace v servisní vrstvě. Pro očekávané chyby (porušení byznys pravidel,
/// validace) se vrací neúspěšný <see cref="Result"/> s chybami – z funkcí by neměly létat výjimky.
/// Komponenty tak mohou chyby snadno zobrazit data i případné chyby.
/// </summary>
public class Result
{
    /// <summary>
    /// Slouží pro ošetření chyb v GUI vrstvě bez Exceptions
    /// </summary>
    public bool Succeeded { get; }

    /// <summary>
    /// Slouží pro ošetření chyb v GUI vrstvě bez Exceptions
    /// </summary>
    public IReadOnlyList<string> Errors { get; }

    protected Result(bool succeeded, IReadOnlyList<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors;
    }

    public static Result Ok() => new(true, []);
    public static Result Err(IReadOnlyList<string> errors) => new(false, errors);
    public static Result Err(params string[] errors) => new(false, errors);


    public static Result<T> Ok<T>(T value) => new(value, true, []);
    public static Result<T> Err<T>(IReadOnlyList<string> errors) => new(default, false, errors);
    public static Result<T> Err<T>(params string[] errors) => new(default, false, errors);
}

/// <summary>Výsledek operace, která při úspěchu vrací hodnotu.</summary>
public sealed class Result<T> : Result
{
    public T Value { get; }

    public Result(T? value, bool succeeded, IReadOnlyList<string> errors)
        : base(succeeded, errors)
    {
        if (succeeded && value is null)
            throw new InvalidOperationException("Result is successful but value is null.");
        Value = value!;
    }

    /// <summary>
    /// Používá se POUZE v rámci gui kontextu, kde se automaticky zachycuje UserFriendlyServiceFailException. Slouží ke kratšímu a přehlednějšímu zpracování stavu v GUI.
    /// </summary>
    /// <exception cref="UserFriendlyServiceFailException"></exception>
    public T Unwrap()
        => Succeeded ? Value : throw new UserFriendlyServiceFailException(Errors);

    /// <summary>Převede úspěšný výsledek na jiný typ; neúspěch se propaguje beze změny (bez výjimky).</summary>
    public Result<U> MapValue<U>(Func<T, U> conversion)
        => Succeeded ? new(conversion(Value!), true, Errors) : new(default, false, Errors);
}

/// <summary>
/// Používá se k shození běhu služby s uživatelsky přívětivou chybou, která se zobrazí.
/// </summary>
/// <param name="errors">Seznam chyb</param>
sealed class UserFriendlyServiceFailException(IReadOnlyList<string> errors) : Exception
{
    internal UserFriendlyServiceFailException(string error) : this([error]) { }
    public IReadOnlyList<string> Errors { get; } = errors;
    public override string Message => string.Join("; ", Errors);
}