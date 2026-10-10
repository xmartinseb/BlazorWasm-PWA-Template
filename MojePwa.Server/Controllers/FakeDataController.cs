using Microsoft.AspNetCore.Mvc;
using MojePwa.Server.Data;

namespace MojePwa.Server.Controllers;

// Note: Controllery by mělý být lehké, jen zpracovávat HTTP záležitosti (routing, binding apod.). Logika má být v servisách

[ApiController]
[Route("api/fake-data")]
public sealed class FakeDataController(FakeDbService fakeDbService) : ControllerBase
{
    [HttpGet]
    public Dictionary<string, string> GetAll()
        => fakeDbService.GetAll();

    [HttpGet("{key}")]
    public string GetValue(string key)
        => fakeDbService.Get(key);

    [HttpPost]
    public IActionResult SetKey([FromBody] KeyValuePair<string, string> data)
    {
        fakeDbService.Set(data.Key, data.Value);
        return Ok();
    }
}