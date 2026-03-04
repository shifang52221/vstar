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
            // Wave 1 fallback: keep runtime stable with WinForms while WinUI host is built incrementally.
            ShellHostMode.WinUiPreview => new WinFormsShellHost(),
            _ => new WinFormsShellHost()
        };
    }
}
