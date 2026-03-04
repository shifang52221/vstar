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

    public AppAgentGateway(IAgentModelRouter? modelRouter = null)
    {
        _modelRouter = modelRouter;
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
}
