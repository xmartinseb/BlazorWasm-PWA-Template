using Microsoft.AspNetCore.Mvc;
using MojePwa.Server.Data;

namespace MojePwa.Server.Controllers;

[ApiController]
[Route("api/fake-data")]
public class FakeDataController(FakeDb fakeDb) : ControllerBase
{
    [HttpGet]
    public Dictionary<string, string> GetAll()
        => fakeDb.Data.ToDictionary();

    [HttpPost]
    public IActionResult SetKey([FromBody] KeyValuePair<string, string> data)
    {
        if (string.IsNullOrEmpty(data.Key)) 
            return BadRequest("Key cannot be null or empty");

        fakeDb.Data[data.Key] = data.Value;
        return Ok();
    }
}