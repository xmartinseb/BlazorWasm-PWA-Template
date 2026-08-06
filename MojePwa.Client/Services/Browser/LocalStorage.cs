using Microsoft.JSInterop;
using MojePwa.Client.Services.DataServices;
using System.Text.Json;

namespace MojePwa.Client.Services.Browser;

public sealed class LocalStorage(IJSRuntime js)
{
    public ValueTask SetAsync<T>(string key, T value)
        => js.InvokeVoidAsync("localStorage.setItem", key, JsonSerializer.Serialize(value));

    public async Task<Result<T>> TryGetAsync<T>(string key)
    {
        if (await js.InvokeAsync<string?>("localStorage.getItem", key) is not string json)
            return Result.Err<T>("Key not found");

        try
        {
            return JsonSerializer.Deserialize<T>(json) switch
            {
                null => Result.Err<T>("Cannot deserialize NULL"),
                var obj => Result.Ok(obj)
            };
        }
        catch
        {
            return Result.Err<T>("Failed to deserialize value");
        }
    }

    public ValueTask RemoveAsync(string key)
        => js.InvokeVoidAsync("localStorage.removeItem", key);

    public ValueTask ClearAsync()
        => js.InvokeVoidAsync("localStorage.clear");
}