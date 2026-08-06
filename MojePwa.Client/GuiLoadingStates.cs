namespace MojePwa.Client;
public interface IGuiState
{
}

/// <summary>
/// Možné přechody: Loading -> Loaded, Error
/// </summary>
public sealed class StateLoading(string message) : IGuiState
{
    public string Message { get; set; } = message;
}

/// <summary>
/// Možné přechody: Loaded -> Reloading (update dat, ale stará data jsou stále zobrazena)
/// Poté: Reloading -> Loaded, Error
/// </summary>
public sealed class StateLoaded<TLoadedData>(TLoadedData loadedData) : IGuiState
{
    public TLoadedData LoadedData { get; set; } = loadedData;

    /// <summary>
    /// Popisuje stav, kdy se zobrazují data a na pozadí běží načítání nových dat (např. po kliknutí na tlačítko "Obnovit").
    /// Bylo by nevhodné přepínat znovu do stavu Loading, protože by uživatel ztratil přehled o starých datech, která jsou stále zobrazena.
    /// </summary>
    public bool IsReloading { get; set; }
}

/// <summary>
/// Možné přechody: Error -> Loading (retry)
/// </summary>
public sealed class StateError(IReadOnlyList<string> errors) : IGuiState
{
    public StateError(string error) : this([error]) { }

    public IReadOnlyList<string> Errors { get; set; } = errors;
    public bool ShowRetryButton { get; set; } = true;
}
