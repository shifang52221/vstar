namespace VStartNext.Core.Agent;

public sealed record AgentPlannerRequest(
    string Input,
    AgentLanguage Language,
    IReadOnlyList<string> AvailableTools);
