namespace VStartNext.Core.Agent;

public sealed record AgentMemoryProfile(
    AgentLanguage PreferredLanguage,
    IReadOnlyDictionary<string, int> ToolFrequency);
