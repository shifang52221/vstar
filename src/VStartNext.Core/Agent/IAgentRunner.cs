namespace VStartNext.Core.Agent;

public interface IAgentRunner
{
    Task<AgentRunResult> RunAsync(string input, bool autoConfirmHighRisk = true);
}
