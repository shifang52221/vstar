using System.Windows.Forms;
using VStartNext.App.Settings;
using VStartNext.App.Win32;
using VStartNext.App.Windows;
using VStartNext.Infrastructure.AI;
using VStartNext.Infrastructure.Security;
using VStartNext.Infrastructure.Storage;

namespace VStartNext.App;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        var modelSettingsService = new ModelSettingsService(
            new AppConfigFileStore(),
            new DpapiSecretProtector(),
            new ModelConnectionTester());

        using var app = new App(enableSystemTrayIcon: true);
        using var shellWindow = new ShellWindowForm();
        shellWindow.SetOpenModelSettingsHandler(() => ShowModelSettingsDialog(shellWindow, modelSettingsService));
        var shellWindowController = new ShellWindowController(shellWindow);
        shellWindow.HideShell();

        app.ShellVisibilityChanged += shellWindowController.ApplyVisibility;
        shellWindow.CommandSubmitted += async (_, input) => await app.HandleCommandInputAsync(input);
        using var window = new HotkeyMessageWindow(app.HandleWindowMessage);
        app.InitializeHotkey(window.Handle);

        Application.Run();
    }

    private static void ShowModelSettingsDialog(
        IWin32Window owner,
        IModelSettingsService modelSettingsService)
    {
        using var dialog = new ModelSettingsForm(modelSettingsService);
        dialog.ShowDialog(owner);
    }
}
