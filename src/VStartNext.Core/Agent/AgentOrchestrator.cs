namespace VStartNext.Core.Agent;

public sealed class AgentOrchestrator
{
    private readonly IAgentPlanner _planner;
    private readonly AgentExecutor _executor;
    private readonly IAgentReflectionService _reflectionService;

    public AgentOrchestrator(
        IAgentPlanner planner,
        AgentExecutor executor,
        IAgentReflectionService reflectionService)
    {
        _planner = planner;
        _executor = executor;
        _reflectionService = reflectionService;
    }

    public async Task<AgentRunResult> RunAsync(string input, bool autoConfirmHighRisk = true)
    {
        var request = new AgentPlannerRequest(input, AgentLanguage.Mixed, []);
        var plan = await _planner.PlanAsync(request);
        var reflectedPlan = await _reflectionService.ReflectAsync(plan);
        return await _executor.ExecuteAsync(reflectedPlan, autoConfirmHighRisk);
    }
}
