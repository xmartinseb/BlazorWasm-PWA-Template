using MojePwa.Client.Services.Browser;
using MojePwa.Client.Services.DataServices;

namespace MojePwa.Client.Models;

/// <summary>
/// View model pro DataComponent, skrze který se datová komponenta ovládá.
/// Podporuje různé režimy načítání a cachování dat.
/// </summary>
/// <param name="dataReloadPeriod">Perioda stahování čerstvých dat ze serveru pomocí timeru. Je-li NULL, reload neprobíhá</param>
/// <param name="cacheUsage">Způsob využití cache v browseru, což může ušetřit přenosy dat. Je-li NULL, nepoužívá se cache</param>
/// <param name="browserTtlCache">Přístup k cache browseru. Pokud je cache usage NOT NULL, toto je taky NOT NULL</param>
public abstract class DataViewModel<TViewModel, TDataLoaded>(TimeSpan? dataReloadPeriod, BrowserCacheUsage? cacheUsage, BrowserTtlCache? browserTtlCache)
    where TViewModel : class
{
    /// <summary>
    /// Tuto eventu zachytává datová komponenta, přes ní se volá na GUI StateHasChanged
    /// </summary>
    public event EventHandler? BlazorStateHasChanged;

    /// <summary>
    /// Vyvolá se po aktualizaci existujícího view modelu novými daty
    /// </summary>
    public event EventHandler? ViewModelUpdated;

    /// <summary>
    /// Zabraňuje vícenásobnému načítání dat najednou
    /// </summary>
    CancellationTokenSource? _loopCts;

    /// <summary>
    /// Stav GUI: Loading, Loaded, Error
    /// </summary>
    public IGuiState? GuiState
    {
        get;
        set
        {
            field = value;
            RaiseBlazorStateHasChanged();
        }
    }

    /// <summary>
    /// Zavolat ve chvíli, kdy je potřeba překreslit datovou komponentu
    /// </summary>
    protected void RaiseBlazorStateHasChanged() => BlazorStateHasChanged?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// Jak často se mají reloadnout data ze serveru pro datovou komponentu
    /// </summary>
    public TimeSpan? DataReloadPeriod { get; init; } = dataReloadPeriod;

    /// <summary>
    /// Je-li NULL, cache prohlížeče se nepoužije. Má-li hodnotu, definuje způsob cachování.
    /// </summary>
    public BrowserCacheUsage? CacheUsage { get; init; } = cacheUsage;

    /// <summary>
    /// Holé načtení dat (typicky skrze nějakou službu)
    /// </summary>
    public abstract Task<Result<TDataLoaded>> LoadFreshDataAsync(CT ct);

    /// <summary>
    /// Vytvoří view model při prvním úspěšném načtení dat
    /// </summary>
    public abstract TViewModel CreateViewModel(TDataLoaded firstLoadedData);

    /// <summary>
    /// Aktualizuje existující view model nově načtenými daty
    /// </summary>
    public abstract void UpdateViewModel(TViewModel currentViewModel, TDataLoaded newData);

    /// <summary>
    /// Zavolá se po kliknutí na tlačítko Retry, které se umí zobrazit v GUI stavu Error
    /// </summary>
    public Task OnRetryClickAsync(CT ct) => LoadAndSetGuiStateAsync("loading...", ct);

    /// <summary>
    /// Načtení dat a přepínání stavů GUI
    /// </summary>
    public async Task LoadAndSetGuiStateAsync(string loadingMessage, CT ct)
    {
        // Pokud už data byla načtena, jen se aktivuje IsReloading. Pokud ještě nejsou, začíná se stavem Loading 
        if (GuiState is not StateLoaded<DataWithTimestamp<TViewModel>>)
            GuiState = new StateLoading(loadingMessage);

        try
        {
            SetIsReloadingIfAlreadyLoaded(true);
            var result = await LoadFromCacheOrLoadFresh(ct);
            if (result.Succeeded)
            {
                if (result.Value is { } data)
                {
                    if (GuiState is StateLoaded<DataWithTimestamp<TViewModel>> loaded)
                    {
                        UpdateViewModel(loaded.LoadedData.Data, data.Data);
                        loaded.LoadedData = new DataWithTimestamp<TViewModel>(loaded.LoadedData.Data, data.Loaded);
                        RaiseBlazorStateHasChanged();
                        ViewModelUpdated?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        GuiState = new StateLoaded<DataWithTimestamp<TViewModel>>(
                            new DataWithTimestamp<TViewModel>(CreateViewModel(data.Data), data.Loaded));
                    }
                }
                else
                {
                    GuiState = new StateError("Byla načtena prázdná odpověď NULL");
                }
            }
            else
                GuiState = new StateError(result.Errors);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (OperationCanceledException)
        {
            GuiState = new StateError("Požadavek byl neočekávaně zrušen.");
        }
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
            if (GuiState is StateLoaded<DataWithTimestamp<TViewModel>> loaded)
            {
                loaded.IsReloading = isReloading;
                RaiseBlazorStateHasChanged();
            }
        }
    }

    /// <returns>
    /// Pokud jsme ve stavu Loaded, vrátí načtená data, jinak NULL
    /// </returns>
    public TViewModel? TryGetViewModel()
        => GuiState is StateLoaded<DataWithTimestamp<TViewModel>> loaded
            ? loaded.LoadedData.Data 
            : default;

    /// <summary>
    /// Rozhodovací procedura: načte se z cache, pokud je to povoleno a jsou tam platná data. Jinak načte ze serveru.
    /// </summary>
    async Task<Result<DataWithTimestamp<TDataLoaded>>> LoadFromCacheOrLoadFresh(CT ct)
    {
        if (CacheUsage is { } c && browserTtlCache is not null)
        {
            var cached = await browserTtlCache.TryGetAsync<TDataLoaded>(c.Storage, c.CacheKey, ct: ct);
            if (cached.Succeeded)
                return Result.Ok(new DataWithTimestamp<TDataLoaded>(cached.Value.CachedValue, cached.Value.Stored));
        }

        var fresh = await LoadFreshDataAsync(ct);
        if (fresh.Succeeded && CacheUsage is { } cu && browserTtlCache is not null)
            await browserTtlCache.StoreAsync(cu.Storage, cu.CacheKey, fresh.Value, cu.CacheTTL, ct);

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
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
    }

    /// <summary>
    /// Vypočítává delay časy pro periodické reloadování dat
    /// </summary>
    TimeSpan GetNextDelay()
    {
        var delay = DataReloadPeriod!.Value;
        if (CacheUsage is { } cu && GuiState is StateLoaded<DataWithTimestamp<TViewModel>> l)
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
        if (CacheUsage is { } cu && browserTtlCache is not null)
            await browserTtlCache.StoreAsync(cu.Storage, cu.CacheKey, modifiedData, cu.CacheTTL, ct);
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