using Microsoft.AspNetCore.Mvc;
using MojePwa.Server.Data;

namespace MojePwa.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class FakeDataController(FakeDb fakeDb) : ControllerBase
{
    [HttpGet(Name = "GetAllData")]
    public Dictionary<string, string> GetAll()
        => fakeDb.Data.ToDictionary();

    [HttpPost(Name = "AddFakeData")]
    public IActionResult AddData([FromBody] KeyValuePair<string, string> data)
    {
        if (string.IsNullOrEmpty(data.Key)) 
            return BadRequest("Key cannot be null or empty");

        fakeDb.Data[data.Key] = data.Value;
        return Ok();
    }
}