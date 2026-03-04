using FluentAssertions;
using VStartNext.Core.Agent;
using Xunit;

namespace VStartNext.Core.Tests.Agent;

public class AgentPolicyGuardTests
{
    [Fact]
    public void Evaluate_HighRiskStep_RequiresConfirmation()
    {
        var guard = new AgentPolicyGuard();

        var decision = guard.Evaluate(new AgentPlanStep("quick_action", "shutdown", AgentRiskLevel.High));

        decision.RequiresUserConfirmation.Should().BeTrue();
    }
}
