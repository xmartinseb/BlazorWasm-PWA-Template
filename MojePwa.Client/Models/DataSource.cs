using MojePwa.Client.Services.Browser;
using MojePwa.Client.Services.DataServices;

namespace MojePwa.Client.Models;

/// <summary>
/// Konfigurační model pro komponentu DataComponent.
/// Definuje načítací proceduru dat, využití cache a nastavení periodického reloadu.
/// </summary>
/// <typeparam name="TData">Typ načítaných dat. Jeho Fullname se defaultně použije jako cache key</typeparam>
public sealed record DataSource<TData>(Func<Task<Result<TData>>> LoadFreshData, TimeSpan? DataReloadPeriod = null, BrowserCacheUsage<TData>? CacheUsage = null);

/// <summary>
/// Nastavení využití cache. Nastavuje se time to live, cache key se umí vybrat automaticky (TData type fullname)
/// </summary>
public readonly struct BrowserCacheUsage<TData>
{
    public BrowserCacheUsage(TimeSpan cacheTtl, BrowserStorageType storage, string? cacheKey = null)
    {
        if (cacheKey == string.Empty)
            throw new ArgumentException("Cache key nemůže být prázdný string", nameof(cacheKey));
        CacheTTL = cacheTtl;
        CacheKey = cacheKey ?? typeof(TData).FullName ?? throw new InvalidOperationException("Typ nemá full name");
        Storage = storage;
    }

    /// <summary>
    /// Cache key se dá specifikovat explicitně, ale často není třeba. Pokud zůstane NULL, vybere se cache key automaticky
    /// </summary>
    public string CacheKey { get; }
    public TimeSpan CacheTTL { get; }
    public BrowserStorageType Storage { get; }
}