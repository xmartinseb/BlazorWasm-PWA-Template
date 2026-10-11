namespace MojePwa.Client.Services.DataServices;

/// <summary>
/// Výsledek operace v servisní vrstvě. Pro očekávané chyby (porušení byznys pravidel,
/// validace) se vrací neúspěšný Result s chybami – z client services vrstvy by neměly létat výjimky.
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


    public static Result<T> Ok<T>(T value) => Result<T>.CreateSuccess(value);
    public static Result<T> Err<T>(IReadOnlyList<string> errors) => Result<T>.CreateFailure(errors);
    public static Result<T> Err<T>(params string[] errors) => Result<T>.CreateFailure(errors);
}

/// <summary>Výsledek operace, která při úspěchu vrací hodnotu.</summary>
public sealed class Result<T> : Result
{
    readonly T? _value;

    Result(T value) : base(true, [])
    {
        if (value is null)
            throw new InvalidOperationException("Result is successful but value is null.");
        _value = value;
    }

    Result(IReadOnlyList<string> errors) : base(false, errors)
    { }

    internal static Result<T> CreateSuccess(T value) => new(value);
    internal static Result<T> CreateFailure(IReadOnlyList<string> errors) => new(errors);

    public T Value
    {
        get
        {
            if (!Succeeded)
                throw new InvalidOperationException("Cannot access the value of a failed result.");

            return _value is { } value
                ? value
                : throw new InvalidOperationException("A successful result must contain a non-null value.");
        }
    }

    /// <summary>
    /// Používá se POUZE v rámci kontextu DataComponent, kde se automaticky zachycuje UserFriendlyServiceFailException. Slouží ke kratšímu a přehlednějšímu zpracování stavu v GUI.
    /// </summary>
    /// <exception cref="UserFriendlyServiceFailException"></exception>
    public T Unwrap()
        => Succeeded ? Value : throw new UserFriendlyServiceFailException(Errors);

    /// <summary>Převede úspěšný výsledek na jiný typ; neúspěch se propaguje beze změny (bez výjimky).</summary>
    public Result<U> MapValue<U>(Func<T, U> conversion)
        => Succeeded ? Result.Ok(conversion(Value)) : Result.Err<U>(Errors);
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