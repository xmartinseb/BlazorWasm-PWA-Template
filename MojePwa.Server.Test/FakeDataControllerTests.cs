using MojePwa.Domain.Dto;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace MojePwa.Server.Test;

public sealed class FakeDataControllerTests : ControllerTestsBase
{
    protected override string ControllerUrl => "/api/fake-data";

    [Fact]
    public Task GetAll_ReturnsValuesFromFakeDataService()
        => DoAsync(async client =>
    {
        var input1 = new SetFakeDbKeyRequestDto(new string('a', SetFakeDbKeyRequestDto.MaxInputLength), "value1");
        var input2 = new SetFakeDbKeyRequestDto(new string('b', SetFakeDbKeyRequestDto.MaxInputLength), "value2");

        AssertNoContent(await client.PutAsJsonAsync(ControllerUrl, input1));
        AssertNoContent(await client.PutAsJsonAsync(ControllerUrl, input2));

        var result = await client.GetFromJsonAsync<Dictionary<string, string>>(ControllerUrl);
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(input1.Value, result[input1.Key]);
        Assert.Equal(input2.Value, result[input2.Key]);
    });

    [Fact]
    public Task GetValue_ReturnsStoredValue()
        => DoAsync(async client =>
    {
        var input = new SetFakeDbKeyRequestDto("name", "value");
        AssertNoContent(await client.PutAsJsonAsync(ControllerUrl, input));

        var response = await client.GetAsync($"{ControllerUrl}/{input.Key}");
        AssertOk(response);
        Assert.Equal(input.Value, await response.Content.ReadAsStringAsync());
    });

    [Fact]
    public Task DeleteKey_ReturnsNoContentThenNotFound()
        => DoAsync(async client =>
    {
        var input = new SetFakeDbKeyRequestDto("name", "value");
        await client.PutAsJsonAsync(ControllerUrl, input);

        AssertNoContent(await client.DeleteAsync($"{ControllerUrl}/{input.Key}"));
        AssertNotFound(await client.DeleteAsync($"{ControllerUrl}/{input.Key}"));
    });

    [Fact]
    public Task SetTooLongKey_Returns400()
        => DoAsync(async client =>
    {
        var input = new SetFakeDbKeyRequestDto(new string('a', SetFakeDbKeyRequestDto.MaxInputLength + 1), "value");
        AssertBadReq(await client.PutAsJsonAsync(ControllerUrl, input));
    });

    [Fact]
    public Task SetTooLongValue_Returns400()
        => DoAsync(async client =>
    {
        var input = new SetFakeDbKeyRequestDto("key", new string('a', SetFakeDbKeyRequestDto.MaxInputLength + 1));
        AssertBadReq(await client.PutAsJsonAsync(ControllerUrl, input));
    });

    [Fact]
    public Task SetKey_StoresValueAndReturnsNoContent()
        => DoAsync(async client =>
    {
        var input = new SetFakeDbKeyRequestDto("name", "value");
        var response = await client.PutAsJsonAsync(ControllerUrl, input);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        response = await client.GetAsync($"{ControllerUrl}/{input.Key}");
        Assert.Equal(input.Value, await response.Content.ReadAsStringAsync());
    });

    [Fact]
    public Task TryAddKey_ReturnsCreatedThenConflict()
        => DoAsync(async client =>
    {
        var input = new SetFakeDbKeyRequestDto("name", "funguje");
        var reInput = input with { Value = "zamitnuto" };

        AssertCreated(await client.PostAsJsonAsync($"{ControllerUrl}/try-add-key", input));
        AssertConflict(await client.PostAsJsonAsync($"{ControllerUrl}/try-add-key", reInput));

        var response = await client.GetAsync($"{ControllerUrl}/{input.Key}");
        AssertOk(response);
        Assert.Equal(input.Value, await response.Content.ReadAsStringAsync());
    });
}