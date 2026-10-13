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
    public Dictionary<string, string> GetAll()
        => fakeDbService.GetAll();

    [HttpGet("{key}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public ActionResult<string> GetValue(string key)
        => fakeDbService.Get(key);

    [HttpDelete("{key}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public IActionResult DeleteKey(string key)
        => fakeDbService.TryRemove(key)
            ? NoContent()
            : NotFound();

    /// <summary>
    /// Idempotentní operace by měly být metoda PUT
    /// </summary>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult SetKey([FromBody] SetFakeDbKeyRequestDto r)
    {
        fakeDbService.Set(r.Key, r.Value);
        return NoContent(); // Note: pro akce, které nevrací data, je toto nejpřesnější success varianta
    }

    /// <summary>
    /// Operace “vytvoř, pokud neexistuje” a vrať bool není čistý idempotentní PUT. Proto je to POST.
    /// Vrací se: 201 Created nebo 409 Conflict. 
    /// Aby to byl čistý PUT, přidal bych header If-None-Match: *, kdy server odpoví 412 Precondition Failed, pokud klíč existuje.
    /// </summary>
    [HttpPost("try-add-key")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult TryAddKey([FromBody] SetFakeDbKeyRequestDto r)
        => fakeDbService.TryAdd(r.Key, r.Value)
            ? Created()
            : Conflict();
}