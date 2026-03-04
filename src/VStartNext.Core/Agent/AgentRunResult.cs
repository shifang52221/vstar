namespace VStartNext.Core.Agent;

public sealed record AgentRunResult(
    bool Success,
    string Message,
    IReadOnlyList<AgentStepExecution> Executions,
    bool RequiresUserConfirmation = false);
