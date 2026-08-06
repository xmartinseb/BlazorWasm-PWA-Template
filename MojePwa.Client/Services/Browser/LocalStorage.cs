using Microsoft.JSInterop;
using System.Text.Json;

namespace MojePwa.Client.Services.Browser;

public sealed class LocalStorage(IJSRuntime js)
{
    public ValueTask SetAsync(string key, string value)
        => js.InvokeVoidAsync("localStorage.setItem", key, value);

    public ValueTask SetAsync<T>(string key, T value)
        => js.InvokeVoidAsync("localStorage.setItem", key, JsonSerializer.Serialize(value));

    public ValueTask<string?> GetAsync(string key)
        => js.InvokeAsync<string?>("localStorage.getItem", key);

    public async Task<T?> GetAsync<T>(string key)
    {
        var json = await js.InvokeAsync<string?>("localStorage.getItem", key);

        if (string.IsNullOrWhiteSpace(json))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (JsonException)
        {
            // Pokud jsou data poškozená, je bezpečnější vrátit default (nebo klíč promazat)
            return default;
        }
    }

    public ValueTask RemoveAsync(string key)
        => js.InvokeVoidAsync("localStorage.removeItem", key);

    public ValueTask ClearAsync()
        => js.InvokeVoidAsync("localStorage.clear");
}