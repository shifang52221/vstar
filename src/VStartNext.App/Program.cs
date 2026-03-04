using System.Windows.Forms;
using VStartNext.App.Agent;
using VStartNext.App.Settings;
using VStartNext.App.Win32;
using VStartNext.App.Windows;
using VStartNext.Core.Agent;
using VStartNext.Infrastructure.Agent;
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

        var configStore = new AppConfigFileStore();
        var secretProtector = new DpapiSecretProtector();
        var modelSettingsService = new ModelSettingsService(
            configStore,
            secretProtector,
            new ModelConnectionTester());
        var modelRouter = new OpenAiCompatibleAgentModelRouter(configStore, secretProtector);
        var tools = new IAgentTool[]
        {
            new LaunchAppAgentTool(),
            new OpenUrlAgentTool(),
            new OpenPathAgentTool()
        };
        var toolRegistry = new AgentToolRegistry(tools);
        var planner = new OpenAiCompatibleAgentPlanner(modelRouter);
        var executor = new AgentExecutor(toolRegistry, new AgentPolicyGuard());
        var reflectionService = new PassThroughAgentReflectionService();
        var orchestrator = new AgentOrchestrator(
            planner,
            executor,
            reflectionService,
            tools.Select(x => x.Name).ToArray());
        var appAgentGateway = new AppAgentGateway(modelRouter, orchestrator);

        using var app = new App(enableSystemTrayIcon: true, agentGateway: appAgentGateway);
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
