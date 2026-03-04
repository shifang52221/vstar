namespace VStartNext.Core.Agent;

public sealed record AgentExecutionPreview(
    string Input,
    IReadOnlyList<AgentPlanStep> Steps);
