using VStartNext.Infrastructure.Win32;
using VStartNext.Core.Launch;
using VStartNext.Core.Search;
using VStartNext.Infrastructure.Launch;
using VStartNext.App.Agent;

namespace VStartNext.App;

public partial class App : IDisposable
{
    private GlobalHotkeyService? _hotkeyService;
    private readonly CommandPaletteService _commandPaletteService;
    private readonly IAppAgentGateway _agentGateway;

    public App(
        bool enableSystemTrayIcon = true,
        ICommandActionExecutor? commandActionExecutor = null,
        IAppAgentGateway? agentGateway = null)
    {
        Tray.Initialize(ToggleShellVisibility, createSystemTrayIcon: enableSystemTrayIcon);
        _commandPaletteService = new CommandPaletteService(commandActionExecutor ?? new LaunchCommandActionExecutor());
        _agentGateway = agentGateway ?? new AppAgentGateway();
    }

    public Services.TrayService Tray { get; } = new();
    public ViewModels.ShellViewModel Shell { get; } = new();
    public CommandExecutionResult? LastCommandResult { get; private set; }
    public event Action<bool>? ShellVisibilityChanged;

    public VStartNext.Core.Abstractions.HotkeyBinding StartupHotkey { get; } =
        VStartNext.Core.Abstractions.HotkeyBinding.Default;

    public string StartupRunKeyPath { get; } =
        VStartNext.Infrastructure.Startup.StartupRegistrationService.RunKeyPath;

    public void InitializeHotkey(nint windowHandle, IWin32HotkeyApi? hotkeyApi = null)
    {
        _hotkeyService = new GlobalHotkeyService(windowHandle, hotkeyApi);
        _hotkeyService.Register(StartupHotkey, ToggleShellVisibility);
    }

    public bool HandleWindowMessage(int message, nuint wParam)
    {
        return _hotkeyService?.TryHandleWindowMessage(message, wParam) ?? false;
    }

    public async Task<CommandExecutionResult> HandleCommandInputAsync(string input)
    {
        CommandExecutionResult result;
        if (_agentGateway.ShouldHandle(input))
        {
            result = await _agentGateway.ExecuteAsync(input);
        }
        else
        {
            result = await _commandPaletteService.ExecuteAsync(input);
        }

        LastCommandResult = result;
        return result;
    }

    public void ToggleShellVisibility()
    {
        Shell.ToggleVisibility();
        ShellVisibilityChanged?.Invoke(Shell.IsVisible);
    }

    public void Dispose()
    {
        _hotkeyService?.Unregister();
        _hotkeyService = null;
        Tray.Dispose();
    }

    private sealed class LaunchCommandActionExecutor : ICommandActionExecutor
    {
        private readonly LaunchExecutor _launchExecutor = new();

        public async Task ExecuteOpenTargetAsync(string target)
        {
            _ = await _launchExecutor.LaunchAsync(new LaunchRequest(target));
        }
    }
}
