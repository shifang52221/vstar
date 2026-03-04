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
            new OpenPathAgentTool(),
            new QuickActionAgentTool()
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
        var auditStore = new AgentAuditFileStore();
        var shellHostMode = ShellHostFactory.ResolveMode(
            Environment.GetEnvironmentVariable("VSTART_SHELL_MODE"));
        using var shellHost = ShellHostFactory.Create(shellHostMode);
        ApplyShellModelProfile(shellHost, modelSettingsService);
        var shellWindow = shellHost.ShellWindow;
        var appAgentGateway = new AppAgentGateway(
            modelRouter,
            orchestrator,
            selectExecutionMode: preview => ShowAgentExecutionPreview(shellHost.OwnerWindow, preview),
            runWithProgress: (preview, planningTokens, run) =>
                ShowAgentExecutionProgress(shellHost.OwnerWindow, modelRouter, preview, planningTokens, run),
            confirmHighRiskAction: message =>
                MessageBox.Show(
                    $"{message}\n\nContinue?",
                    "AI Safety Confirmation",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning) == DialogResult.Yes,
            auditStore: auditStore,
            uiLanguage: System.Globalization.CultureInfo.CurrentUICulture.Name,
            followUiLanguage: false);

        using var app = new App(enableSystemTrayIcon: true, agentGateway: appAgentGateway);
        shellHost.SetOpenModelSettingsHandler(() =>
        {
            ShowModelSettingsDialog(shellHost.OwnerWindow, modelSettingsService);
            ApplyShellModelProfile(shellHost, modelSettingsService);
        });
        var shellWindowController = new ShellWindowController(shellWindow);
        shellWindow.HideShell();

        app.ShellVisibilityChanged += shellWindowController.ApplyVisibility;
        shellHost.CommandSubmitted += async (_, input) => await app.HandleCommandInputAsync(input);
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

    private static AgentExecutionMode ShowAgentExecutionPreview(
        IWin32Window owner,
        AgentExecutionPreview preview)
    {
        using var dialog = new AgentExecutionPreviewForm(preview);
        dialog.ShowDialog(owner);
        return dialog.SelectedMode;
    }

    private static Task<AgentRunResult> ShowAgentExecutionProgress(
        IWin32Window owner,
        IAgentModelRouter modelRouter,
        AgentExecutionPreview preview,
        IReadOnlyList<string> planningTokens,
        Func<CancellationToken, IProgress<AgentExecutionUpdate>, Task<AgentRunResult>> run)
    {
        using var dialog = new AgentExecutionProgressForm(
            preview,
            run,
            planningTokenStream: cancellationToken =>
                ReplayTokensAsync(planningTokens, cancellationToken),
            finalizingTokenStream: (result, cancellationToken) =>
                modelRouter.StreamCompletionAsync(
                    AgentFinalizingPromptBuilder.Build(
                        preview,
                        result,
                        uiLanguage: System.Globalization.CultureInfo.CurrentUICulture.Name,
                        followUiLanguage: false),
                    cancellationToken));
        dialog.ShowDialog(owner);
        return Task.FromResult(dialog.RunResult ?? new AgentRunResult(false, "Execution canceled", []));
    }

    private static async IAsyncEnumerable<string> ReplayTokensAsync(
        IReadOnlyList<string> tokens,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var token in tokens)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return token;
            await Task.CompletedTask;
        }
    }

    private static void ApplyShellModelProfile(
        IAppShellHost shellHost,
        IModelSettingsService modelSettingsService)
    {
        var input = modelSettingsService.Load();
        shellHost.SetModelProfile(input.Provider, input.ChatModel);
    }
}
