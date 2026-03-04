namespace VStartNext.Core.Agent;

public sealed class AgentExecutor
{
    private readonly AgentToolRegistry _toolRegistry;
    private readonly AgentPolicyGuard _policyGuard;

    public AgentExecutor(AgentToolRegistry toolRegistry, AgentPolicyGuard policyGuard)
    {
        _toolRegistry = toolRegistry;
        _policyGuard = policyGuard;
    }

    public async Task<AgentRunResult> ExecuteAsync(AgentActionPlan plan, bool autoConfirmHighRisk = false)
    {
        var executions = new List<AgentStepExecution>();

        foreach (var step in plan.Steps)
        {
            var decision = _policyGuard.Evaluate(step);
            if (decision.RequiresUserConfirmation && !autoConfirmHighRisk)
            {
                return new AgentRunResult(false, "Confirmation required", executions);
            }

            var toolResult = await _toolRegistry.ExecuteAsync(step.ToolName, step.Arguments);
            var execution = new AgentStepExecution(step.ToolName, step.Arguments, toolResult.Success, toolResult.Message);
            executions.Add(execution);

            if (!toolResult.Success)
            {
                return new AgentRunResult(false, toolResult.Message, executions);
            }
        }

        return new AgentRunResult(true, "Completed", executions);
    }
}
