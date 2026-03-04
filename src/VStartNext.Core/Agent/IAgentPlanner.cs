namespace VStartNext.Core.Agent;

public interface IAgentPlanner
{
    Task<AgentActionPlan> PlanAsync(
        AgentPlannerRequest request,
        IProgress<string>? planningProgress = null,
        CancellationToken cancellationToken = default);
}
