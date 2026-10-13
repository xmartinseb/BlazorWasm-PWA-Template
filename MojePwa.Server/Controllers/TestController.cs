using Microsoft.AspNetCore.Mvc;
using MojePwa.Server.Services.Exceptions;

namespace MojePwa.Server.Controllers;

[ApiController]
[Route("api/test")]
public sealed class TestController: ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public string GetRandomResult()
        => Random.Shared.Next(5) switch
        {
            0 => "API text response",
            2 => throw new UserFriendlyForbiddenException("TEST forbidden message"),
            3 => throw new UserFriendlyNotFoundException("TEST not found"),
            4 => throw new UserFriendlyUnauthorizedException("TEST unauthorized message"),
            _ => throw new ArgumentException("TEST argument message", "TestParameterName"),
        };
}