namespace VStartNext.Core.Agent;

public sealed record AgentPolicyDecision(bool IsAllowed, bool RequiresUserConfirmation);
