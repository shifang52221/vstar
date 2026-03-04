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

    public AppAgentGateway(IAgentModelRouter? modelRouter = null, IAgentRunner? agentRunner = null)
    {
        _modelRouter = modelRouter;
        _agentRunner = agentRunner;
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
            var runResult = await _agentRunner!.RunAsync(input, autoConfirmHighRisk: true);
            if (!runResult.Success)
            {
                return new CommandExecutionResult(false, runResult.Message);
            }

            if (runResult.Executions.Count == 0)
            {
                return new CommandExecutionResult(true, runResult.Message);
            }

            var summary = string.Join(", ", runResult.Executions.Select(x => $"{x.ToolName}({x.Arguments})"));
            return new CommandExecutionResult(true, $"{runResult.Message}: {summary}");
        }
        catch (Exception ex)
        {
            return new CommandExecutionResult(false, $"Agent execution failed: {ex.Message}");
        }
    }
}
