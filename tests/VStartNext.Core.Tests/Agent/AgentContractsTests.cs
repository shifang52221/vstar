using FluentAssertions;
using VStartNext.Core.Agent;
using Xunit;

namespace VStartNext.Core.Tests.Agent;

public class AgentContractsTests
{
    [Fact]
    public void AgentPlan_CanRepresentMultiStepBilingualIntent()
    {
        var plan = new AgentActionPlan(
            AgentIntent.Automation,
            "打开 Chrome and open github",
            [new AgentPlanStep("launch_app", "chrome", AgentRiskLevel.Low)]);

        plan.Steps.Should().HaveCount(1);
        plan.Intent.Should().Be(AgentIntent.Automation);
    }
}
