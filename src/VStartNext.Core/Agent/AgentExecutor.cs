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

    public async Task<AgentRunResult> ExecuteAsync(
        AgentActionPlan plan,
        bool autoConfirmHighRisk = false,
        int? maxSteps = null,
        CancellationToken cancellationToken = default)
    {
        var executions = new List<AgentStepExecution>();
        if (!autoConfirmHighRisk)
        {
            var pendingHighRiskStep = plan.Steps.FirstOrDefault(step =>
                _policyGuard.Evaluate(step).RequiresUserConfirmation);
            if (pendingHighRiskStep is not null)
            {
                return new AgentRunResult(
                    false,
                    $"Confirmation required for {pendingHighRiskStep.ToolName}({pendingHighRiskStep.Arguments})",
                    executions,
                    RequiresUserConfirmation: true);
            }
        }

        var steps = maxSteps.HasValue
            ? plan.Steps.Take(Math.Max(0, maxSteps.Value))
            : plan.Steps;

        foreach (var step in steps)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return new AgentRunResult(false, "Execution canceled", executions);
            }

            var decision = _policyGuard.Evaluate(step);
            if (!decision.IsAllowed)
            {
                return new AgentRunResult(false, $"Action not allowed: {step.ToolName}", executions);
            }

            if (decision.RequiresUserConfirmation && !autoConfirmHighRisk)
            {
                return new AgentRunResult(
                    false,
                    $"Confirmation required for {step.ToolName}({step.Arguments})",
                    executions,
                    RequiresUserConfirmation: true);
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
