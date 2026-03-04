namespace VStartNext.Core.Agent;

public sealed class AgentToolRegistry
{
    private readonly Dictionary<string, IAgentTool> _tools;

    public AgentToolRegistry(IEnumerable<IAgentTool> tools)
    {
        _tools = tools.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
    }

    public Task<AgentToolResult> ExecuteAsync(string toolName, string arguments)
    {
        if (!_tools.TryGetValue(toolName, out var tool))
        {
            return Task.FromResult(new AgentToolResult(false, $"Unknown tool: {toolName}"));
        }

        return tool.ExecuteAsync(arguments);
    }
}
