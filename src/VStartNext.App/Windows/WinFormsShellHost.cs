using System.Windows.Forms;

namespace VStartNext.App.Windows;

public sealed class WinFormsShellHost : IAppShellHost
{
    private readonly ShellWindowForm _shellWindow;

    public WinFormsShellHost()
    {
        _shellWindow = new ShellWindowForm();
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
