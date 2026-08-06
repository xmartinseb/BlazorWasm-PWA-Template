using Microsoft.JSInterop;
using MojePwa.Client.Services.DataServices;
using System.Text.Json;

namespace MojePwa.Client.Services.Browser;

/// <summary>
/// Prostý reader/writer pro local cache, jen obaluje Javascript do čitelných funkcí
/// </summary>
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

/// <summary>
/// Je postaven nad LocalStorage. Na záznamy nahlíží jako na cached objekty s TTL (time-to-live). 
/// Záznamy se ukládají do LocalStorage a při čtení se kontroluje, zda ještě nejsou prošlé.
/// </summary>
public sealed class LocalTtlCache(LocalStorage localStorage)
{
    public ValueTask StoreAsync<T>(string key, T value, TimeSpan ttl)
    {
        var entry = new CacheEntry<T>(value, DateTime.UtcNow, ttl);
        return localStorage.SetAsync(key, entry);
    }

    public async Task<Result<T>> TryGetAsync<T>(string key, bool readExpired = false)
    {
        var result = await localStorage.TryGetAsync<CacheEntry<T>>(key);
        if (!result.Succeeded)
            return Result.Err<T>(result.Errors);
        var entry = result.Value;
        if (!readExpired && entry.IsExpired())
        {
            await localStorage.RemoveAsync(key);
            return Result.Err<T>("Cache expired");
        }

        return Result.Ok(entry.Value);
    }

    public readonly record struct CacheEntry<T>(T Value, DateTime StoredUtc, TimeSpan Ttl)
    {
        public bool IsExpired() => DateTime.UtcNow - StoredUtc > Ttl;
    }
}