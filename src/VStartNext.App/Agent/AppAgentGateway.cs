using VStartNext.Core.Agent;
using VStartNext.Core.Search;

namespace VStartNext.App.Agent;

public interface IAppAgentGateway
{
    bool ShouldHandle(string input);

    Task<CommandExecutionResult> ExecuteAsync(string input);
}

public sealed class AppAgentGateway : IAppAgentGateway
{
    private static readonly string[] Prefixes = ["calc:", "url:", "ws:"];
    private readonly IAgentModelRouter? _modelRouter;
    private readonly IAgentRunner? _agentRunner;
    private readonly Func<AgentExecutionPreview, AgentExecutionMode>? _selectExecutionMode;
    private readonly Func<
        AgentExecutionPreview,
        Func<CancellationToken, IProgress<AgentExecutionUpdate>, Task<AgentRunResult>>,
        Task<AgentRunResult>>? _runWithProgress;
    private readonly Func<string, bool>? _confirmHighRiskAction;
    private readonly IAgentAuditStore? _auditStore;
    private readonly AgentResponseLanguagePolicy _languagePolicy = new();
    private readonly string _uiLanguage;
    private readonly bool _followUiLanguage;

    public AppAgentGateway(
        IAgentModelRouter? modelRouter = null,
        IAgentRunner? agentRunner = null,
        Func<AgentExecutionPreview, AgentExecutionMode>? selectExecutionMode = null,
        Func<
            AgentExecutionPreview,
            Func<CancellationToken, IProgress<AgentExecutionUpdate>, Task<AgentRunResult>>,
            Task<AgentRunResult>>? runWithProgress = null,
        Func<string, bool>? confirmHighRiskAction = null,
        IAgentAuditStore? auditStore = null,
        string uiLanguage = "zh-CN",
        bool followUiLanguage = false)
    {
        _modelRouter = modelRouter;
        _agentRunner = agentRunner;
        _selectExecutionMode = selectExecutionMode;
        _runWithProgress = runWithProgress;
        _confirmHighRiskAction = confirmHighRiskAction;
        _auditStore = auditStore;
        _uiLanguage = uiLanguage;
        _followUiLanguage = followUiLanguage;
    }

    public bool ShouldHandle(string input)
    {
        var value = (input ?? string.Empty).Trim();
        if (value.Length == 0)
        {
            return false;
        }

        if (Prefixes.Any(prefix => value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        var hasWhitespace = value.Contains(' ');
        var hasCjk = value.Any(ch => ch >= '\u4e00' && ch <= '\u9fff');
        return hasWhitespace || hasCjk;
    }

    public Task<CommandExecutionResult> ExecuteAsync(string input)
    {
        if (_agentRunner is not null)
        {
            return ExecuteWithRunnerAsync(input);
        }

        if (_modelRouter is null)
        {
            return Task.FromResult(new CommandExecutionResult(false, "Agent gateway is not configured"));
        }

        return ExecuteWithRouterAsync(input);
    }

    private async Task<CommandExecutionResult> ExecuteWithRouterAsync(string input)
    {
        try
        {
            var output = await _modelRouter!.CompleteAsync(input);
            return new CommandExecutionResult(true, output);
        }
        catch (Exception ex)
        {
            return new CommandExecutionResult(false, $"Agent request failed: {ex.Message}");
        }
    }

    private async Task<CommandExecutionResult> ExecuteWithRunnerAsync(string input)
    {
        try
        {
            var preview = await _agentRunner!.PreviewAsync(input);
            var mode = _selectExecutionMode?.Invoke(preview) ?? AgentExecutionMode.ExecuteAll;
            if (mode == AgentExecutionMode.Cancel)
            {
                AppendAudit(preview, mode, false, "Action canceled by user.", []);
                return new CommandExecutionResult(false, "Action canceled by user.");
            }

            int? maxSteps = mode == AgentExecutionMode.ExecuteSingleStep ? 1 : null;
            var runResult = await RunCoreAsync(preview, maxSteps, autoConfirmHighRisk: false);
            if (runResult.RequiresUserConfirmation)
            {
                if (_confirmHighRiskAction is null)
                {
                    AppendAudit(preview, mode, false, runResult.Message, []);
                    return new CommandExecutionResult(false, runResult.Message);
                }

                var approved = _confirmHighRiskAction(runResult.Message);
                if (!approved)
                {
                    AppendAudit(preview, mode, false, "Action canceled by user.", []);
                    return new CommandExecutionResult(false, "Action canceled by user.");
                }

                runResult = await RunCoreAsync(preview, maxSteps, autoConfirmHighRisk: true);
            }

            if (!runResult.Success)
            {
                AppendAudit(
                    preview,
                    mode,
                    false,
                    runResult.Message,
                    runResult.Executions.Select(FormatExecutionLine));
                return new CommandExecutionResult(false, runResult.Message);
            }

            AppendAudit(
                preview,
                mode,
                true,
                runResult.Message,
                runResult.Executions.Select(FormatExecutionLine));
            return new CommandExecutionResult(true, FormatExecutionOutput(runResult, input));
        }
        catch (Exception ex)
        {
            AppendAudit(
                new AgentExecutionPreview(input, []),
                AgentExecutionMode.ExecuteAll,
                false,
                $"Agent execution failed: {ex.Message}",
                []);
            return new CommandExecutionResult(false, $"Agent execution failed: {ex.Message}");
        }

        async Task<AgentRunResult> RunCoreAsync(
            AgentExecutionPreview preview,
            int? maxSteps,
            bool autoConfirmHighRisk)
        {
            if (_runWithProgress is null)
            {
                return await _agentRunner.RunAsync(
                    preview,
                    autoConfirmHighRisk: autoConfirmHighRisk,
                    maxSteps: maxSteps,
                    cancellationToken: CancellationToken.None,
                    progress: NullProgress.Instance);
            }

            return await _runWithProgress(
                preview,
                (cancellationToken, progress) => _agentRunner.RunAsync(
                    preview,
                    autoConfirmHighRisk: autoConfirmHighRisk,
                    maxSteps: maxSteps,
                    cancellationToken: cancellationToken,
                    progress: progress));
        }
    }

    private string FormatExecutionOutput(AgentRunResult runResult, string input)
    {
        var language = _languagePolicy.Resolve(input, _uiLanguage, _followUiLanguage);
        if (runResult.Executions.Count == 0)
        {
            return ResolveHeader(runResult.Success, language);
        }

        var trace = runResult.Executions
            .Select((execution, index) => $"{index + 1}. {FormatExecutionLine(execution)}");
        return $"{ResolveHeader(runResult.Success, language)}\n{string.Join("\n", trace)}";
    }

    private static string FormatExecutionLine(AgentStepExecution execution)
    {
        return $"{execution.ToolName}({execution.Arguments}) => {execution.Message}";
    }

    private static string ResolveHeader(bool success, AgentLanguage language)
    {
        if (success)
        {
            return language switch
            {
                AgentLanguage.Chinese => "\u5df2\u5b8c\u6210",
                AgentLanguage.English => "Completed",
                _ => "Completed / \u5df2\u5b8c\u6210"
            };
        }

        return language switch
        {
            AgentLanguage.Chinese => "\u6267\u884c\u5931\u8d25",
            AgentLanguage.English => "Failed",
            _ => "Failed / \u6267\u884c\u5931\u8d25"
        };
    }

    private void AppendAudit(
        AgentExecutionPreview preview,
        AgentExecutionMode mode,
        bool success,
        string message,
        IEnumerable<string> steps)
    {
        if (_auditStore is null)
        {
            return;
        }

        _auditStore.Append(new AgentAuditEntry(
            DateTimeOffset.Now,
            preview.Input,
            mode,
            success,
            message,
            steps.ToArray()));
    }

    private sealed class NullProgress : IProgress<AgentExecutionUpdate>
    {
        public static readonly NullProgress Instance = new();

        public void Report(AgentExecutionUpdate value)
        {
        }
    }
}
