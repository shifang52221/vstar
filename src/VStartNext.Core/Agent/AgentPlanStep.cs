namespace VStartNext.Core.Agent;

public sealed record AgentPlanStep(string ToolName, string Arguments, AgentRiskLevel RiskLevel);
