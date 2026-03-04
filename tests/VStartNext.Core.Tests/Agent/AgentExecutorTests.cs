using FluentAssertions;
using VStartNext.Core.Agent;
using Xunit;

namespace VStartNext.Core.Tests.Agent;

public class AgentExecutorTests
{
    [Fact]
    public async Task ExecuteAsync_RunsAllStepsInOrder()
    {
        var registry = new AgentToolRegistry(
        [
            new FakeTool("launch_app"),
            new FakeTool("open_url")
        ]);
        var executor = new AgentExecutor(registry, new AgentPolicyGuard());
        var plan = new AgentActionPlan(
            AgentIntent.Automation,
            "open chrome and github",
            [
                new AgentPlanStep("launch_app", "chrome", AgentRiskLevel.Low),
                new AgentPlanStep("open_url", "https://github.com", AgentRiskLevel.Low)
            ]);

        var result = await executor.ExecuteAsync(plan, autoConfirmHighRisk: true);

        result.Success.Should().BeTrue();
        result.Executions.Select(x => x.ToolName).Should().Equal("launch_app", "open_url");
    }

    private sealed class FakeTool : IAgentTool
    {
        public FakeTool(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public Task<AgentToolResult> ExecuteAsync(string arguments)
        {
            return Task.FromResult(new AgentToolResult(true, $"ok:{arguments}"));
        }
    }
}
