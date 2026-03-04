using System.Windows.Forms;
using VStartNext.App.Win32;
using VStartNext.App.Windows;

namespace VStartNext.App;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        using var app = new App(enableSystemTrayIcon: true);
        using var shellWindow = new ShellWindowForm();
        var shellWindowController = new ShellWindowController(shellWindow);
        shellWindow.HideShell();

        app.ShellVisibilityChanged += shellWindowController.ApplyVisibility;
        shellWindow.CommandSubmitted += async (_, input) => await app.HandleCommandInputAsync(input);
        using var window = new HotkeyMessageWindow(app.HandleWindowMessage);
        app.InitializeHotkey(window.Handle);

        Application.Run();
    }
}
