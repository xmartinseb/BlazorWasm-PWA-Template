using MojePwa.Client.Dialogs.Common;
using Radzen;

namespace MojePwa.Client.Extensions;

public static class DialogServiceExtensions
{
    /// <summary>
    /// Jednoduché zobrazení základního dialogu Yes/No.
    /// </summary>
    /// <returns>
    /// true pro Yes, false pro No a null pokud uživatel dialog zavřel
    /// </returns>
    public static async Task<bool?> YesNoAsync(this DialogService dialogService, string message, string title = "", string? yesText = null, string? noText = null)
    {
        var parameters = new Dictionary<string, object?>
        {
            [nameof(YesNoDialog.YesText)] = yesText ?? "Ano",
            [nameof(YesNoDialog.NoText)] = noText ?? "Ne"
        };

        parameters[nameof(YesNoDialog.Message)] = message;
        return await dialogService.OpenAsync<YesNoDialog>(title, parameters);
    }
}
