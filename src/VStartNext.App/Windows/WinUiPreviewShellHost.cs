using System.Windows.Forms;

namespace VStartNext.App.Windows;

public sealed class WinUiPreviewShellHost : IAppShellHost
{
    private readonly ShellWindowForm _shellWindow;

    public WinUiPreviewShellHost()
    {
        _shellWindow = new ShellWindowForm();
        _shellWindow.Text = "VStart Next (WinUI Preview Host)";
        _shellWindow.CommandSubmitted += ForwardCommandSubmitted;
    }

    public IShellWindow ShellWindow => _shellWindow;

    public IWin32Window OwnerWindow => _shellWindow;

    public event EventHandler<string>? CommandSubmitted;

    public void SetOpenModelSettingsHandler(Action onOpenModelSettings)
    {
        _shellWindow.SetOpenModelSettingsHandler(onOpenModelSettings);
    }

    public void Dispose()
    {
        _shellWindow.CommandSubmitted -= ForwardCommandSubmitted;
        _shellWindow.Dispose();
    }

    private void ForwardCommandSubmitted(object? sender, string input)
    {
        CommandSubmitted?.Invoke(this, input);
    }
}
