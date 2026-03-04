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

    public async Task<AgentExecutionPreview> PreviewAsync(string input)
    {
        var request = new AgentPlannerRequest(input, AgentLanguage.Mixed, _availableTools);
        var plan = await _planner.PlanAsync(request);
        var reflectedPlan = await _reflectionService.ReflectAsync(plan);
        return new AgentExecutionPreview(input, reflectedPlan.Steps);
    }

    public async Task<AgentRunResult> RunAsync(
        AgentExecutionPreview preview,
        bool autoConfirmHighRisk = true,
        int? maxSteps = null,
        CancellationToken cancellationToken = default)
    {
        var plan = new AgentActionPlan(
            AgentIntent.Automation,
            preview.Input,
            preview.Steps);
        return await _executor.ExecuteAsync(
            plan,
            autoConfirmHighRisk,
            maxSteps,
            cancellationToken);
    }

    public async Task<AgentRunResult> RunAsync(string input, bool autoConfirmHighRisk = true)
    {
        var preview = await PreviewAsync(input);
        return await RunAsync(preview, autoConfirmHighRisk);
    }
}
