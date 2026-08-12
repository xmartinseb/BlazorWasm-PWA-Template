using MojePwa.Client.Services.DataServices;

namespace MojePwa.Client.Services.Browser;

/// <summary>
/// Používá se jako dependency injection. Má přístup ke storage browseru a umí vytvářet cachované hodnoty.
/// </summary>
public sealed class BrowserStoredValueFactory(BrowserStorage storage)
{
    public BrowserStoredValue<T> Create<T>(string cacheKey, BrowserStorageType storageType)
        => new(storage, cacheKey, storageType);
}

/// <summary>
/// Slouží k jednoduché manipulaci s hodnotou, která sídlí v úložišti browseru
/// </summary>
/// <typeparam name="T">Typ hodnoty, v browseru se ukládá jako JSON</typeparam>
/// <param name="storage">Manipulátor s úložištěm browseru</param>
/// <param name="storageKey">Klíč hodnoty v rámci svého browser úložiště</param>
public sealed class BrowserStoredValue<T>(BrowserStorage storage, string storageKey, BrowserStorageType storageType)
{
    public T? Value { get; private set; } = default;

    public async Task SetAsync(T value)
    {
        await storage.SetAsync(storageType, storageKey, value);
        Value = value;
    }

    public Task<Result<T>> LoadAsync()
        => LoadInternalAsync(false);

    public Task<Result<T>> LoadAsync(T fallbackValue)
        => LoadInternalAsync(true, fallbackValue);

    async Task<Result<T>> LoadInternalAsync(bool useFallback, T fallbackValue = default!)
    {
        var result = useFallback
            ? await storage.TryGetAsync(storageType, storageKey, fallbackValue)
            : await storage.TryGetAsync<T>(storageType, storageKey);
        if (!result.Succeeded)
            return Result.Err<T>(result.Errors);
        Value = result.Value;
        return Result.Ok(Value);
    }
}