using VStartNext.Core.Agent;

namespace VStartNext.Infrastructure.Agent;

public sealed class PassThroughAgentReflectionService : IAgentReflectionService
{
    public Task<AgentActionPlan> ReflectAsync(AgentActionPlan plan)
    {
        return Task.FromResult(plan);
    }
}
