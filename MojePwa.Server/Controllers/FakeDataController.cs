using Microsoft.AspNetCore.Mvc;
using MojePwa.Domain.Dto;
using MojePwa.Server.Services;

namespace MojePwa.Server.Controllers;

// Note: Controllery by mělý být lehké, jen zpracovávat HTTP záležitosti (routing, binding apod.). Logika má být v servisách

[ApiController]
[Route("api/fake-data")]
public sealed class FakeDataController(FakeDataService fakeDbService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(Dictionary<string, string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public Dictionary<string, string> GetAll()
        => fakeDbService.GetAll();

    [HttpGet("{key}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public string GetValue(string key)
        => fakeDbService.Get(key);

    /// <summary>
    /// Idempotentní operace by měly být metoda PUT
    /// </summary>
    [HttpPut("set-key")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public IActionResult SetKey([FromBody] SetFakeDbKeyRequestDto r)
    {
        fakeDbService.Set(r.Key, r.Value);
        return NoContent(); // Note: pro akce, které nevrací data, je toto nejpřesnější success varianta
    }

    /// <summary>
    /// Idempotentní operace by měly být metoda PUT
    /// </summary>
    [HttpPut("try-add-key")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public bool TryAddKey([FromBody] SetFakeDbKeyRequestDto r) 
        => fakeDbService.TryAdd(r.Key, r.Value);
}