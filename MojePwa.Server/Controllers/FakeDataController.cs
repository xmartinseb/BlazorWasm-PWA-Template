using Microsoft.AspNetCore.Mvc;
using MojePwa.Server.Data;

namespace MojePwa.Server.Controllers;

// Note: Controllery by mělý být lehké, jen zpracovávat HTTP záležitosti (routing, binding apod.). Logika má být v servisách

[ApiController]
[Route("api/fake-data")]
public sealed class FakeDataController(FakeDbService fakeDbService) : ControllerBase
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

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public IActionResult SetKey([FromBody] KeyValuePair<string, string> data)
    {
        fakeDbService.Set(data.Key, data.Value);
        return NoContent(); // Note: pro akce, které nevrací data, je toto nejpřesnější varianta
    }
}