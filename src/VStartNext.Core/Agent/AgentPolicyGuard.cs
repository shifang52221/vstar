namespace VStartNext.Core.Agent;

public sealed class AgentPolicyGuard
{
    public AgentPolicyDecision Evaluate(AgentPlanStep step)
    {
        var requiresConfirmation = step.RiskLevel == AgentRiskLevel.High;
        return new AgentPolicyDecision(IsAllowed: true, RequiresUserConfirmation: requiresConfirmation);
    }
}
