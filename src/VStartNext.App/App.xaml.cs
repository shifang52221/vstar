namespace VStartNext.App;

public partial class App
{
    public Services.TrayService Tray { get; } = new();

    public VStartNext.Core.Abstractions.HotkeyBinding StartupHotkey { get; } =
        VStartNext.Core.Abstractions.HotkeyBinding.Default;

    public string StartupRunKeyPath { get; } =
        VStartNext.Infrastructure.Startup.StartupRegistrationService.RunKeyPath;
}
