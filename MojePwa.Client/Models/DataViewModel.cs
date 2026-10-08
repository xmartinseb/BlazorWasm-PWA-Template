using MojePwa.Client.Services.Browser;
using MojePwa.Client.Services.DataServices;

namespace MojePwa.Client.Models;

/// <summary>
/// View model s logikou pro DataComponent
/// </summary>
public abstract class DataViewModel<TDataLoaded>(TimeSpan? dataReloadPeriod, BrowserCacheUsage? cacheUsage, BrowserTtlCache? browserTtlCache)
{
    public event EventHandler? BlazorStateHasChanged;
    CancellationTokenSource? _loopCts;

    public IGuiState? GuiState
    {
        get => field;
        set
        {
            field = value;
            RaiseBlazorStateHasChanged();
        }
    }

    protected void RaiseBlazorStateHasChanged() => BlazorStateHasChanged?.Invoke(this, EventArgs.Empty);

    public TimeSpan? DataReloadPeriod { get; init; } = dataReloadPeriod;

    /// <summary>
    /// Je-li NULL, cache prohlížeče se nepoužije. Má-li hodnotu, definuje způsob cachování.
    /// </summary>
    public BrowserCacheUsage? CacheUsage { get; init; } = cacheUsage;

    /// <summary>
    /// Holé načtení dat (typicky skrze nějakou službu)
    /// </summary>
    public abstract Task<Result<TDataLoaded>> LoadFreshDataAsync(CT ct);

    public Task OnRetryClickAsync() => LoadAndSetGuiStateAsync("loading...", CT.None);

    /// <summary>
    /// Načtení dat a přepínání stavů GUI
    /// </summary>
    public async Task LoadAndSetGuiStateAsync(string loadingMessage, CT ct)
    {
        // Pokud už data byla načtena, jen se aktivuje IsReloading. Pokud ještě nejsou, začíná se stavem Loading 
        if (GuiState is not StateLoaded<DataWithTimestamp<TDataLoaded>>)
            GuiState = new StateLoading(loadingMessage);

        try
        {
            SetIsReloadingIfAlreadyLoaded(true);
            var result = await LoadFromCacheOrLoadFresh(ct);
            if (result.Succeeded)
                GuiState = result.Value is { } data
                    ? new StateLoaded<DataWithTimestamp<TDataLoaded>>(data)
                    : new StateError("Byla načtena prázdná odpověď NULL");
            else
                GuiState = new StateError(result.Errors);
        }
        catch (OperationCanceledException) { throw; }
        catch (UserFriendlyServiceFailException ex)
        {
            GuiState = new StateError(ex.Errors);
        }
        catch (Exception)
        {
            // Sem by to padat nemělo
            GuiState = new StateError("Neznámá chyba");
        }
        finally
        {
            SetIsReloadingIfAlreadyLoaded(false);
        }

        // Pomocná: pokud jsou již data načtena, zobrazí/skryje se pod komponentou loading progress bar
        void SetIsReloadingIfAlreadyLoaded(bool isReloading)
        {
            if (GuiState is StateLoaded<DataWithTimestamp<TDataLoaded>> loaded)
            {
                loaded.IsReloading = isReloading;
                RaiseBlazorStateHasChanged();
            }
        }
    }

    public TDataLoaded? TryGetLoadedData() 
        => GuiState is StateLoaded<DataWithTimestamp<TDataLoaded>> loaded 
            ? loaded.LoadedData.Data 
            : default;

    async Task<Result<DataWithTimestamp<TDataLoaded>>> LoadFromCacheOrLoadFresh(CT ct)
    {
        if (CacheUsage is { } c && browserTtlCache is not null)
        {
            var cached = await browserTtlCache.TryGetAsync<TDataLoaded>(c.Storage, c.CacheKey);
            if (cached.Succeeded)
                return Result.Ok(new DataWithTimestamp<TDataLoaded>(cached.Value.CachedValue, cached.Value.Stored));
        }

        var fresh = await LoadFreshDataAsync(ct);
        if (fresh.Succeeded && CacheUsage is { } cu && browserTtlCache is not null)
            await browserTtlCache.StoreAsync(cu.Storage, cu.CacheKey, fresh.Value, cu.CacheTTL);

        return fresh.MapValue(_fresh => new DataWithTimestamp<TDataLoaded>(_fresh, DateTimeOffset.UtcNow));
    }

    public void StartPeriodicReload()
    {
        if (DataReloadPeriod is null || _loopCts is not null)
            return;

        _loopCts = new CancellationTokenSource();
        _ = RunPeriodicAsync(_loopCts.Token);
    }

    public void StopPeriodicReload()
    {
        _loopCts?.Cancel();
        _loopCts?.Dispose();
        _loopCts = null;
    }

    async Task RunPeriodicAsync(CT ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                await LoadAndSetGuiStateAsync("Loading...", ct);
                await Task.Delay(GetNextDelay(), ct);
            }
        }
        catch (OperationCanceledException) { }
    }

    TimeSpan GetNextDelay()
    {
        var delay = DataReloadPeriod!.Value;
        if (CacheUsage is { } cu && GuiState is StateLoaded<DataWithTimestamp<TDataLoaded>> l)
        {
            var untilExpiry = l.LoadedData.Loaded + cu.CacheTTL - DateTimeOffset.UtcNow + TimeSpan.FromMilliseconds(20);
            if (untilExpiry > delay) delay = untilExpiry;
        }
        return delay;
    }

    /// <summary>
    /// Uloží upravená data do cache
    /// </summary>
    public async Task StoreCacheAsync(TDataLoaded modifiedData, CT ct)
    {
        // TODO: use CT in caches?
        if (CacheUsage is { } cu && browserTtlCache is not null)
            await browserTtlCache.StoreAsync(cu.Storage, cu.CacheKey, modifiedData, cu.CacheTTL);
    }
}

/// <summary>
/// Nastavení využití cache. Nastavuje se time to live a cache key
/// </summary>
public readonly struct BrowserCacheUsage
{
    public BrowserCacheUsage(TimeSpan cacheTtl, BrowserStorageType storage, string cacheKey)
    {
        if (cacheKey == string.Empty)
            throw new ArgumentException("Cache key nemůže být prázdný string", nameof(cacheKey));
        CacheTTL = cacheTtl;
        CacheKey = cacheKey;
        Storage = storage;
    }

    public string CacheKey { get; }
    public TimeSpan CacheTTL { get; }
    public BrowserStorageType Storage { get; }
}

public sealed record DataWithTimestamp<TDataLoaded>(TDataLoaded Data, DateTimeOffset Loaded);