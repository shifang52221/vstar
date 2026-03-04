namespace VStartNext.Core.Agent;

public interface IAgentTool
{
    string Name { get; }

    Task<AgentToolResult> ExecuteAsync(string arguments);
}
