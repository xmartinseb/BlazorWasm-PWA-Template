using Microsoft.AspNetCore.Components;
using MojePwa.Client.Services.DataServices;

namespace MojePwa.Client.Components.Utils;

/// <summary>
/// Základ pro komponenty, které pomocí services načítají Result data, zobrazují loading, loaded a případně Error.
/// Komponenta neobsahuje přímo child StateSwitch, ani nevolá automaticky načítací metody (bylo by to málo flexibilní)
/// Slouží jen k DRY ohledně přepínání stavu komponenty
/// </summary>
/// <typeparam name="TDataLoaded">Typ dat, který má odvozená komponenta po načtení zobrazovat</typeparam>
public abstract class ComponentWithGuiState<TDataLoaded> : ComponentBase
{
    /// <summary>
    /// Stav, který musí odvozené komponenty explicitně používat, typicky ve svém StateSwitch
    /// </summary>
    protected IGuiState? GuiState { get; set; }

    protected abstract Task<Result<TDataLoaded>> LoadDataAsync();

    protected abstract string LoadingMessage { get; set; }

    /// <summary>
    /// Volá pod sebou načítání dat a mění stav komponenty.
    /// Tuto metodu musí odvozené komponenty volat explicitně (kvůli flexibilitě)
    /// </summary>
    protected async Task LoadAndSetGuiStateAsync()
    {
        // Pokud už data byla načtena, jen se aktivuje IsReloading. Pokud ještě nejsou, začíná se stavem Loading 
        if (GuiState is not StateLoaded<TDataLoaded>)
            GuiState = new StateLoading(LoadingMessage);

        try
        {
            SetIsReloadingIfAlreadyLoaded(true);
            var result = await LoadDataAsync();
            if (result.Succeeded)
                GuiState = result.Value is { } data ? new StateLoaded<TDataLoaded>(data) : new StateError("Byla načtena prázdná odpověď NULL");
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
    }

    void SetIsReloadingIfAlreadyLoaded(bool isReloading)
    {
        if (GuiState is StateLoaded<TDataLoaded> loaded)
        {
            loaded.IsReloading = isReloading;
            StateHasChanged();
        }
    }
}