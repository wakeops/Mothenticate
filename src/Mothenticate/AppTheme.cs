using MudBlazor;

namespace Mothenticate;

internal static class AppTheme
{
    internal static readonly MudTheme Default = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#5C6BC0",
            Secondary = "#7E57C2",
            AppbarBackground = "#3F51B5"
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#7986CB",
            Secondary = "#9575CD",
            AppbarBackground = "#1a1a2e",
            Background = "#121212",
            BackgroundGray = "#1a1a2e",
            Surface = "#1e1e2e",
            DrawerBackground = "#1a1a2e",
            DrawerText = "rgba(255,255,255,0.7)",
            DrawerIcon = "rgba(255,255,255,0.7)"
        }
    };
}
