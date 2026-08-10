using Microsoft.JSInterop;
using MojePwa.Client.Services.DataServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MojePwa.Client.Services.Browser;

/// <summary>
/// Deskriptor k odlišení local a session storage - webové prohlížeče obsahují oba dva.
/// Local storage přežije zavření prohlížeče a je sdílen mezi všemi okny a záložkami STEJNÉHO ORIGIN (protokol, doména, port)
///
/// Session storage se smaže po zavření záložky. Navíc není sdílena mezi záložkami, takže je vhodná pro krátkodobá data, která mají být izolovaná na jednu záložku.
/// Díky vazbě na záložku přežije refresh stránky F5 a přesměrování na externí stránku a zpět - zásadní výhody oproti prosté aplikační in memory cache
/// </summary>
public enum BrowserStorageType { Local, Session }

/// <summary>
/// Prostý reader/writer pro local cache, jen obaluje Javascript do čitelných funkcí.
/// </summary>
public sealed class BrowserStorage(IJSRuntime js)
{
    static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public ValueTask SetAsync<T>(BrowserStorageType s, string key, T value)
    {
        var json = JsonSerializer.Serialize(value, Options);
        return js.InvokeVoidAsync($"{GetStorageName(s)}.setItem", key, json);
    }

    public async Task<Result<T>> TryGetAsync<T>(BrowserStorageType s, string key)
    {
        if (await js.InvokeAsync<string?>($"{GetStorageName(s)}.getItem", key) is not string json)
            return Result.Err<T>("Key not found");

        try
        {
            return JsonSerializer.Deserialize<T>(json, Options) switch
            {
                null => Result.Err<T>("Cannot deserialize NULL"),
                var obj => Result.Ok(obj)
            };
        }
        catch (JsonException)
        {
            return Result.Err<T>("Failed to deserialize value");
        }
    }

    public ValueTask RemoveAsync(BrowserStorageType s, string key)
        => js.InvokeVoidAsync($"{GetStorageName(s)}.removeItem", key);

    public ValueTask ClearAsync(BrowserStorageType s)
        => js.InvokeVoidAsync($"{GetStorageName(s)}.clear");

    static string GetStorageName(BrowserStorageType s) => s switch
    {
        BrowserStorageType.Local => "localStorage",
        BrowserStorageType.Session => "sessionStorage",
        _ => throw new ArgumentOutOfRangeException(nameof(s))
    };
}


/// <summary>
/// Je postaven nad BrowserStorage. Na uložené hodnoty nahlíží jako na cached objekty s TTL (time-to-live). 
/// Záznamy se ukládají do LocalStorage nebo SessionStorage a při čtení se kontroluje, zda ještě nejsou expirované.
/// </summary>
public sealed class BrowserTtlCache(BrowserStorage storage)
{
    public ValueTask StoreAsync<T>(BrowserStorageType s, string key, T value, TimeSpan ttl)
    {
        var entry = new BrowserCacheEntry<T>(value, DateTimeOffset.UtcNow, ttl);
        return storage.SetAsync(s, key, entry);
    }

    public async Task<Result<BrowserCacheEntry<T>>> TryGetAsync<T>(BrowserStorageType s, string key, bool readExpiredEnabled = false)
    {
        var result = await storage.TryGetAsync<BrowserCacheEntry<T>>(s, key);
        if (!result.Succeeded)
            return Result.Err<BrowserCacheEntry<T>>(result.Errors);
        var entry = result.Value;
        if (!readExpiredEnabled && entry.IsExpired)
        {
            await storage.RemoveAsync(s, key);
            return Result.Err<BrowserCacheEntry<T>>("Cache expired");
        }

        return Result.Ok(entry);
    }

    public ValueTask RemoveAsync(BrowserStorageType s, string cacheKey)
        => storage.RemoveAsync(s, cacheKey);
}


/// <summary>
/// Data v cache s časem vytvoření a TTL, který určuje čas expirace
/// </summary>
public readonly record struct BrowserCacheEntry<T>(T CachedValue, DateTimeOffset Stored, TimeSpan Ttl)
{
    [JsonIgnore]
    public bool IsExpired => DateTimeOffset.UtcNow - Stored > Ttl;
}