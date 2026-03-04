namespace VStartNext.Core.Agent;

public sealed record AgentAuditEntry(
    DateTimeOffset Timestamp,
    string Input,
    AgentExecutionMode Mode,
    bool Success,
    string Message,
    IReadOnlyList<string> Steps);
