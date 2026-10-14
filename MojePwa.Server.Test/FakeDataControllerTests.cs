using Microsoft.AspNetCore.Mvc;
using MojePwa.Domain.Dto;
using MojePwa.Server.Controllers;
using MojePwa.Server.Services;
using Xunit;

namespace MojePwa.Server.Test;

public sealed class FakeDataControllerTests
{
    [Fact]
    public void GetAll_ReturnsValuesFromFakeDataService()
    {
        var controller = new FakeDataController(new FakeDataService());
        var key1 = new string('a', SetFakeDbKeyRequestDto.MaxInputLength);
        var key2 = new string('b', SetFakeDbKeyRequestDto.MaxInputLength);

        Assert.IsType<NoContentResult>(controller.SetKey(new SetFakeDbKeyRequestDto(key1, "value1")));
        Assert.IsType<NoContentResult>(controller.SetKey(new SetFakeDbKeyRequestDto(key2, "value2")));

        var result = controller.GetAll();
        Assert.Equal(2, result.Count);
        Assert.Equal("value1", result[key1]);
        Assert.Equal("value2", result[key2]);
    }

    [Fact]
    public void GetValue_ReturnsStoredValue()
    {
        var controller = new FakeDataController(new FakeDataService());
        controller.SetKey(new SetFakeDbKeyRequestDto("name", "value"));

        var result = controller.GetValue("name");
        Assert.Equal("value", result.Value);
    }

    [Fact]
    public void DeleteKey_ReturnsNoContentThenNotFound()
    {
        var controller = new FakeDataController(new FakeDataService());
        controller.SetKey(new SetFakeDbKeyRequestDto("name", "value"));

        var deleteResult = controller.DeleteKey("name");
        var missingResult = controller.DeleteKey("name");

        Assert.IsType<NoContentResult>(deleteResult);
        Assert.IsType<NotFoundResult>(missingResult);
    }

    [Fact]
    public void SetTooLongKey_Returns400()
    {
        var controller = new FakeDataController(new FakeDataService());
        var tooLongKey = new string('a', SetFakeDbKeyRequestDto.MaxInputLength + 1);
        var result = controller.SetKey(new SetFakeDbKeyRequestDto(tooLongKey, "a"));
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public void SetTooLongValue_Returns400()
    {
        var controller = new FakeDataController(new FakeDataService());
        var tooLongValue = new string('a', SetFakeDbKeyRequestDto.MaxInputLength + 1);
        var result = controller.SetKey(new SetFakeDbKeyRequestDto("a", tooLongValue));
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public void SetKey_StoresValueAndReturnsNoContent()
    {
        var controller = new FakeDataController(new FakeDataService());
        var result = controller.SetKey(new SetFakeDbKeyRequestDto("name", "value"));

        Assert.IsType<NoContentResult>(result);
        Assert.Equal("value", controller.GetValue("name").Value);
    }

    [Fact]
    public void TryAddKey_ReturnsCreatedThenConflict()
    {
        var controller = new FakeDataController(new FakeDataService());
        var request = new SetFakeDbKeyRequestDto("name", "first");

        var createdResult = controller.TryAddKey(request);
        var conflictResult = controller.TryAddKey(new SetFakeDbKeyRequestDto("name", "second"));

        Assert.Equal(201, Assert.IsType<CreatedResult>(createdResult).StatusCode);
        Assert.IsType<ConflictResult>(conflictResult);
        Assert.Equal("first", controller.GetValue("name").Value);
    }
}
