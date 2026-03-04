namespace VStartNext.Core.Agent;

public sealed record AgentExecutionUpdate(
    int StepIndex,
    int TotalSteps,
    string ToolName,
    string Arguments,
    string State,
    string Message);
