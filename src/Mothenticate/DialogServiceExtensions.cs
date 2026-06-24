using MudBlazor;
using Mothenticate.Components.Shared;

namespace Mothenticate;

internal static class DialogServiceExtensions
{
    internal static async Task<bool?> ConfirmDeleteAsync(this IDialogService dialogs, string title, string message)
    {
        var parameters = new DialogParameters<ConfirmDeleteDialog>
        {
            { d => d.Title, title },
            { d => d.Message, message }
        };
        var options = new DialogOptions { MaxWidth = MaxWidth.ExtraSmall, FullWidth = true };
        var dialog = await dialogs.ShowAsync<ConfirmDeleteDialog>(string.Empty, parameters, options);
        var result = await dialog.Result;
        return result is { Canceled: false } ? true : null;
    }
}
