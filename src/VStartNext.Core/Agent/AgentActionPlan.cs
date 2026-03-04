namespace VStartNext.Core.Agent;

public sealed record AgentActionPlan(
    AgentIntent Intent,
    string OriginalInput,
    IReadOnlyList<AgentPlanStep> Steps);
