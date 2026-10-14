namespace MojePwa.Client;
public interface IGuiState
{
}

/// <summary>
/// Stav, kdy komponenta načítá data a zobrazuje loading zprávu
/// </summary>
public sealed class StateLoading(string message) : IGuiState
{
    public string Message { get; set; } = message;
}

/// <summary>
/// Stav, kdy komponenta zobrazuje načtená data
/// </summary>
public sealed class StateLoaded<TLoadedData>(TLoadedData loadedData) : IGuiState
{
    public TLoadedData LoadedData { get; set; } = loadedData;

    /// <summary>
    /// Reload dat ve stavu Loaded se nedělá zpátky přes stav Loading, protože by zmizela dříve načtená data.
    /// Místo toho se pod načtenými daty při reloadingu zobrazuje progress bar.
    /// Když je TRUE, je zobrazen ten progress bar.
    /// </summary>
    public bool IsReloading { get; set; }
}

/// <summary>
/// Stav, kdy je uživateli zobrazena chyba načítání.
/// Je možné načíst data znovu přes Reload button, pokud se nabízí.
/// </summary>
public sealed class StateError(IReadOnlyList<string> errors) : IGuiState
{
    public StateError(string error) : this([error]) { }

    public IReadOnlyList<string> Errors { get; set; } = errors;
    public bool ShowRetryButton { get; set; } = true;
}
