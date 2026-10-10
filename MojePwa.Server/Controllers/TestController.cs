using Microsoft.AspNetCore.Mvc;
using MojePwa.Server.Data;
using MojePwa.Server.Services.Exceptions;

namespace MojePwa.Server.Controllers;

[ApiController]
[Route("api/test")]
public sealed class TestController: ControllerBase
{
    [HttpGet]
    public string GetRandomResult()
        => Random.Shared.Next(5) switch
        {
            0 => "API text response",
            2 => throw new UserFriendlyForbiddenException("TEST forbidden message"),
            3 => throw new UserFriendlyNotFoundException("TEST not found"),
            4 => throw new UserFriendlyUnauthrorizedException("TEST unauthorized message"),
            _ => throw new ArgumentException("TEST argument message", "TestParameterName"),
        };
}