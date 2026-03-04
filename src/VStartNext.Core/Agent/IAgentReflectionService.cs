namespace VStartNext.Core.Agent;

public interface IAgentReflectionService
{
    Task<AgentActionPlan> ReflectAsync(AgentActionPlan plan);
}
