namespace VStartNext.App;

public partial class App
{
    public App(bool enableSystemTrayIcon = true)
    {
        Tray.Initialize(() => Shell.ToggleVisibility(), createSystemTrayIcon: enableSystemTrayIcon);
    }

    public Services.TrayService Tray { get; } = new();
    public ViewModels.ShellViewModel Shell { get; } = new();

    public VStartNext.Core.Abstractions.HotkeyBinding StartupHotkey { get; } =
        VStartNext.Core.Abstractions.HotkeyBinding.Default;

    public string StartupRunKeyPath { get; } =
        VStartNext.Infrastructure.Startup.StartupRegistrationService.RunKeyPath;
}
