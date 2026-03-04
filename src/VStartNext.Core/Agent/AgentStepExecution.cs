namespace VStartNext.Core.Agent;

public sealed record AgentStepExecution(
    string ToolName,
    string Arguments,
    bool Success,
    string Message);
