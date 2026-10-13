using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace MojePwa.Server.Services.Exceptions;

/// <summary>
/// Vyhazuje na NULL nějakého parametru, který je nastavován konzumentem API. Proto je jistota, že je obsažená zpráva
/// o chybném argumentu publikovatelná (na rozdíl od System.ArgumentNullException, které se může vyhodit odkudkoliv a obsahovat tak citlivé informace o kódu)
/// </summary>
public class UserFriendlyArgumentNullException(string paramName, string? messageForUser = null) 
    : UserFriendlyArgumentException(paramName, messageForUser ?? $"Argument {paramName} cannot be NULL")
{
    public override string Title => "Argument NULL error";

    public static void ThrowIfNull([NotNull] object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (argument is null) throw new UserFriendlyArgumentNullException(paramName ?? "");
    }
}


/// <summary>
/// Vyhazuje se, když dojde k nějaké chybě u parametru, který je nastavován konzumentem API. Proto je jistota, že je obsažená zpráva
/// o chybném argumentu publikovatelná (na rozdíl od System.ArgumentException, které se může vyhodit odkudkoliv a obsahovat tak citlivé informace o kódu)
/// </summary>
public class UserFriendlyArgumentException(string paramName, string messageForUser) : UserFriendlyException(messageForUser, null)
{
    public override int HttpStatus => 400;
    public override string Title => "Argument error";
    public string ParamName { get; } = paramName;
}

public class UserFriendlyNotFoundException(string messageForUser, Exception? inner = null) : UserFriendlyException(messageForUser, inner)
{
    public override int HttpStatus => 404;
    public override string Title => "Requested resource not found";
}

/// <summary>
/// Uživatel je přihlášen, ale nemá práva
/// </summary>
public class UserFriendlyForbiddenException(string messageForUser, Exception? inner = null) : UserFriendlyException(messageForUser, inner)
{
    public override int HttpStatus => 403;
    public override string Title => "Forbidden";
}

/// <summary>
/// Uživatel neni přihlášen
/// </summary>
public class UserFriendlyUnauthorizedException(string messageForUser, Exception? inner = null) : UserFriendlyException(messageForUser, inner)
{
    public override int HttpStatus => 401;
    public override string Title => "Unauthrorized";
}

public class UserFriendlyInvalidRequestException(string messageForUser, Exception? inner = null) : UserFriendlyException(messageForUser, inner)
{
    public override int HttpStatus => 400;
    public override string Title => "Invalid request";
}

/// <summary>
/// Tyto výjimky se vyznačují tím, že obsahují zprávy, které se dají vracet z API ven.
/// Dojde-li k jejich vyhození v service či controlleru, middleware ApiExceptionHandler je automaticky zachytí.
/// </summary>
/// <param name="messageForUser">Uživatelsky čitelná bezpečná zpráva, která neobsahuje žádné citlivé údaje z kódu</param>
/// <param name="inner">Původní chyba, která se nikam nevystavuje, jen se zaloguje</param>
public abstract class UserFriendlyException(string messageForUser, Exception? inner) : Exception(messageForUser, inner)
{
    public abstract int HttpStatus { get; }
    public abstract string Title { get; }
}