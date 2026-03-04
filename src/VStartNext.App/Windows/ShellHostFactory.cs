namespace VStartNext.App.Windows;

public static class ShellHostFactory
{
    public static ShellHostMode ResolveMode(string? modeValue)
    {
        var normalized = (modeValue ?? string.Empty).Trim();
        if (normalized.Equals("winui-preview", StringComparison.OrdinalIgnoreCase))
        {
            return ShellHostMode.WinUiPreview;
        }

        return ShellHostMode.WinForms;
    }

    public static IAppShellHost Create(ShellHostMode mode)
    {
        return mode switch
        {
            ShellHostMode.WinUiPreview => CreateWinUiPreviewHostWithFallback(),
            _ => new WinFormsShellHost()
        };
    }

    private static IAppShellHost CreateWinUiPreviewHostWithFallback()
    {
        try
        {
            return new WinUiPreviewShellHost();
        }
        catch
        {
            return new WinFormsShellHost();
        }
    }
}
