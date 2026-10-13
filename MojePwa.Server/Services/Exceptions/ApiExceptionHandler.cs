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
            ArgumentNullException => (StatusCodes.Status400BadRequest, "Bad Request", "A required argument is missing"),
            ArgumentOutOfRangeException => (StatusCodes.Status400BadRequest, "Bad Request", "An argument is outside the permitted range"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Bad Request", "An argument is invalid"),
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Forbidden", "You are not allowed to perform this operation"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Conflict", "The request could not be completed in the current state"),
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