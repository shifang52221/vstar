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
        var auditStore = new AgentAuditFileStore();
        using var shellWindow = new ShellWindowForm();
        var appAgentGateway = new AppAgentGateway(
            modelRouter,
            orchestrator,
            selectExecutionMode: preview => ShowAgentExecutionPreview(shellWindow, preview),
            runWithProgress: (preview, planningTokens, run) =>
                ShowAgentExecutionProgress(shellWindow, modelRouter, preview, planningTokens, run),
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
                modelRouter.StreamCompletionAsync(BuildFinalizingPrompt(preview, result), cancellationToken));
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

    private static string BuildFinalizingPrompt(AgentExecutionPreview preview, AgentRunResult result)
    {
        var executions = result.Executions.Count == 0
            ? "No executed steps."
            : string.Join(
                "; ",
                result.Executions.Select((execution, index) =>
                    $"{index + 1}. {execution.ToolName}({execution.Arguments}) => {execution.Message}"));
        return
            "You are an assistant. Output concise final summary text in plain language only. " +
            $"User input: {preview.Input}. Success: {result.Success}. Message: {result.Message}. Executions: {executions}.";
    }
}
