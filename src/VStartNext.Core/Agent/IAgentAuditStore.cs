namespace VStartNext.Core.Agent;

public interface IAgentAuditStore
{
    void Append(AgentAuditEntry entry);

    IReadOnlyList<AgentAuditEntry> LoadRecent();
}
