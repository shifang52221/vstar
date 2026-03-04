namespace VStartNext.Core.Agent;

public interface IAgentPlanner
{
    Task<AgentActionPlan> PlanAsync(AgentPlannerRequest request);
}
