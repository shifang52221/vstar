using System.Windows.Forms;

namespace VStartNext.App.Windows;

public interface IAppShellHost : IDisposable
{
    IShellWindow ShellWindow { get; }

    IWin32Window OwnerWindow { get; }

    event EventHandler<string>? CommandSubmitted;

    void SetOpenModelSettingsHandler(Action onOpenModelSettings);
}
