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
    private readonly Func<string, bool>? _confirmHighRiskAction;
    private readonly AgentResponseLanguagePolicy _languagePolicy = new();
    private readonly string _uiLanguage;
    private readonly bool _followUiLanguage;

    public AppAgentGateway(
        IAgentModelRouter? modelRouter = null,
        IAgentRunner? agentRunner = null,
        Func<string, bool>? confirmHighRiskAction = null,
        string uiLanguage = "zh-CN",
        bool followUiLanguage = false)
    {
        _modelRouter = modelRouter;
        _agentRunner = agentRunner;
        _confirmHighRiskAction = confirmHighRiskAction;
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
            var runResult = await _agentRunner!.RunAsync(input, autoConfirmHighRisk: false);
            if (runResult.RequiresUserConfirmation)
            {
                if (_confirmHighRiskAction is null)
                {
                    return new CommandExecutionResult(false, runResult.Message);
                }

                var approved = _confirmHighRiskAction(runResult.Message);
                if (!approved)
                {
                    return new CommandExecutionResult(false, "Action canceled by user.");
                }

                runResult = await _agentRunner.RunAsync(input, autoConfirmHighRisk: true);
            }

            if (!runResult.Success)
            {
                return new CommandExecutionResult(false, runResult.Message);
            }

            return new CommandExecutionResult(true, FormatExecutionOutput(runResult, input));
        }
        catch (Exception ex)
        {
            return new CommandExecutionResult(false, $"Agent execution failed: {ex.Message}");
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
            .Select((execution, index) => $"{index + 1}. {execution.ToolName}({execution.Arguments}) => {execution.Message}");
        return $"{ResolveHeader(runResult.Success, language)}\n{string.Join("\n", trace)}";
    }

    private static string ResolveHeader(bool success, AgentLanguage language)
    {
        if (success)
        {
            return language switch
            {
                AgentLanguage.Chinese => "已完成",
                AgentLanguage.English => "Completed",
                _ => "Completed / 已完成"
            };
        }

        return language switch
        {
            AgentLanguage.Chinese => "执行失败",
            AgentLanguage.English => "Failed",
            _ => "Failed / 执行失败"
        };
    }
}
