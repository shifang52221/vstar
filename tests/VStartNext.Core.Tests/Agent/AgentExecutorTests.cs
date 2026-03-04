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

    [Fact]
    public async Task ExecuteAsync_WhenHighRiskExistsWithoutAutoConfirm_ReturnsConfirmationRequestBeforeExecution()
    {
        var tool = new CountingTool("quick_action");
        var registry = new AgentToolRegistry([tool]);
        var executor = new AgentExecutor(registry, new AgentPolicyGuard());
        var plan = new AgentActionPlan(
            AgentIntent.Automation,
            "shutdown",
            [new AgentPlanStep("quick_action", "shutdown", AgentRiskLevel.High)]);

        var result = await executor.ExecuteAsync(plan, autoConfirmHighRisk: false);

        result.Success.Should().BeFalse();
        result.RequiresUserConfirmation.Should().BeTrue();
        tool.ExecuteCount.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_WithMaxSteps_OnlyRunsRequestedStepCount()
    {
        var first = new CountingTool("launch_app");
        var second = new CountingTool("open_url");
        var registry = new AgentToolRegistry([first, second]);
        var executor = new AgentExecutor(registry, new AgentPolicyGuard());
        var plan = new AgentActionPlan(
            AgentIntent.Automation,
            "open chrome and openai",
            [
                new AgentPlanStep("launch_app", "chrome", AgentRiskLevel.Low),
                new AgentPlanStep("open_url", "https://openai.com", AgentRiskLevel.Low)
            ]);

        var result = await executor.ExecuteAsync(plan, autoConfirmHighRisk: true, maxSteps: 1);

        result.Success.Should().BeTrue();
        result.Executions.Should().HaveCount(1);
        first.ExecuteCount.Should().Be(1);
        second.ExecuteCount.Should().Be(0);
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

    private sealed class CountingTool : IAgentTool
    {
        public CountingTool(string name)
        {
            Name = name;
        }

        public string Name { get; }
        public int ExecuteCount { get; private set; }

        public Task<AgentToolResult> ExecuteAsync(string arguments)
        {
            ExecuteCount++;
            return Task.FromResult(new AgentToolResult(true, $"ok:{arguments}"));
        }
    }
}
