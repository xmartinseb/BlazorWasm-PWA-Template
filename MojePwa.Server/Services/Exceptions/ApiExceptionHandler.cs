using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MojePwa.Server.Services.Exceptions;

/// <summary>
/// Globální handler výjimek pro API. Zachytává neošetřené výjimky z celé pipeline
/// a převádí je na jednotnou odpověď ve formátu Problem Details (RFC 9457).
/// Díky tomu není potřeba psát try/catch v každé akci kontroleru.
/// </summary>
/// <param name="problemDetailsService">Služba pro zápis Problem Details do odpovědi. Generuje formát RFC9457</param>
/// <param name="logger">Logger pro zaznamenání zachycených výjimek.</param>
public sealed class ApiExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<ApiExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception ex, CT ct)
    {
        var (status, title, detail) = ex switch
        {
            UserFriendlyException uex => (uex.HttpStatus, uex.Title, uex.Message),
            ArgumentNullException anex => (StatusCodes.Status400BadRequest, "Argument cannot be NULL", $"Argument {anex.ParamName} cannot be NULL"),
            ArgumentOutOfRangeException arex => (StatusCodes.Status400BadRequest, "Argument out of range", $"Argument out of range: {arex.ParamName}"),
            ArgumentException aex => (StatusCodes.Status400BadRequest, "Invalid argument", $"Invalid argument: {aex.ParamName}"),
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Forbidden", "You are not allowed to perform this operation"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Conflict", ex.Message),
            _ => (StatusCodes.Status500InternalServerError, "Server error", "An unexpected error occurred")
        };

        logger.LogError(ex, "Request failed with status {StatusCode}", status);
        httpContext.Response.StatusCode = status;

        // Do odpovědi se zapíše info o chybě ve formátu RFC9457
        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail,
                Instance = httpContext.Request.Path,
                Extensions = { ["traceId"] = httpContext.TraceIdentifier }
            }
        });

        return true; // = exception handled successfully
    }
}