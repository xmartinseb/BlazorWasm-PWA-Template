namespace MojePwa.Server.Services.Exceptions;

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
public class UserFriendlyUnauthrorizedException(string messageForUser, Exception? inner = null) : UserFriendlyException(messageForUser, inner)
{
    public override int HttpStatus => 401;
    public override string Title => "Unauthrorized";
}

/// <summary>
/// Tyto výjimky se vyznačují tím, že obsahují zprávy, které se dají vracet z API ven.
/// Dojde-li k jejich vyhození v service či controlleru, middleware ApiExceptionHandler je automaticky zachytí.
/// </summary>
/// <param name="messageForUser">Bezpečná zpráva, která se dá vrátit ven z API a neobsahuje žádné citlivé údaje</param>
/// <param name="inner">Původní chyba, která se nikam nevystavuje, jen se zaloguje</param>
public abstract class UserFriendlyException(string messageForUser, Exception? inner) : Exception(messageForUser, inner)
{
    public abstract int HttpStatus { get; }
    public abstract string Title { get; }
}