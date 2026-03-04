namespace VStartNext.Core.Agent;

public sealed class AgentOrchestrator : IAgentRunner
{
    private readonly IAgentPlanner _planner;
    private readonly AgentExecutor _executor;
    private readonly IAgentReflectionService _reflectionService;
    private readonly IReadOnlyList<string> _availableTools;

    public AgentOrchestrator(
        IAgentPlanner planner,
        AgentExecutor executor,
        IAgentReflectionService reflectionService,
        IReadOnlyList<string>? availableTools = null)
    {
        _planner = planner;
        _executor = executor;
        _reflectionService = reflectionService;
        _availableTools = availableTools ?? [];
    }

    public async Task<AgentRunResult> RunAsync(string input, bool autoConfirmHighRisk = true)
    {
        var request = new AgentPlannerRequest(input, AgentLanguage.Mixed, _availableTools);
        var plan = await _planner.PlanAsync(request);
        var reflectedPlan = await _reflectionService.ReflectAsync(plan);
        return await _executor.ExecuteAsync(reflectedPlan, autoConfirmHighRisk);
    }
}
