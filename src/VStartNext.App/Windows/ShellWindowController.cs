namespace VStartNext.App.Windows;

public sealed class ShellWindowController
{
    private readonly IShellWindow _shellWindow;

    public ShellWindowController(IShellWindow shellWindow)
    {
        _shellWindow = shellWindow;
    }

    public void ApplyVisibility(bool isVisible)
    {
        if (isVisible)
        {
            _shellWindow.ShowShell();
            return;
        }

        _shellWindow.HideShell();
    }
}
