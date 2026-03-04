using VStartNext.Infrastructure.Win32;

namespace VStartNext.App;

public partial class App : IDisposable
{
    private GlobalHotkeyService? _hotkeyService;

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

    public void InitializeHotkey(nint windowHandle, IWin32HotkeyApi? hotkeyApi = null)
    {
        _hotkeyService = new GlobalHotkeyService(windowHandle, hotkeyApi);
        _hotkeyService.Register(StartupHotkey, () => Shell.ToggleVisibility());
    }

    public bool HandleWindowMessage(int message, nuint wParam)
    {
        return _hotkeyService?.TryHandleWindowMessage(message, wParam) ?? false;
    }

    public void Dispose()
    {
        _hotkeyService?.Unregister();
        _hotkeyService = null;
        Tray.Dispose();
    }
}
