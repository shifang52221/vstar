using FluentAssertions;
using VStartNext.Core.Agent;
using Xunit;

namespace VStartNext.Core.Tests.Agent;

public class AgentOrchestratorTests
{
    [Fact]
    public async Task RunAsync_InvokesReflectionBeforeExecution()
    {
        var planner = new FakePlanner();
        var reflection = new FakeReflectionService();
        var registry = new AgentToolRegistry([new FakeTool("launch_app")]);
        var executor = new AgentExecutor(registry, new AgentPolicyGuard());
        var orchestrator = new AgentOrchestrator(planner, executor, reflection);

        var result = await orchestrator.RunAsync("打开 chrome");

        reflection.Called.Should().BeTrue();
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task RunAsync_PassesAvailableToolsToPlannerRequest()
    {
        var planner = new CapturingPlanner();
        var reflection = new FakeReflectionService();
        var registry = new AgentToolRegistry([new FakeTool("launch_app")]);
        var executor = new AgentExecutor(registry, new AgentPolicyGuard());
        var orchestrator = new AgentOrchestrator(planner, executor, reflection, ["launch_app", "open_url"]);

        await orchestrator.RunAsync("open chrome");

        planner.LastRequest.Should().NotBeNull();
        planner.LastRequest!.AvailableTools.Should().Contain("launch_app");
        planner.LastRequest.AvailableTools.Should().Contain("open_url");
    }

    private sealed class FakePlanner : IAgentPlanner
    {
        public Task<AgentActionPlan> PlanAsync(AgentPlannerRequest request)
        {
            var plan = new AgentActionPlan(
                AgentIntent.Automation,
                request.Input,
                [new AgentPlanStep("launch_app", "chrome", AgentRiskLevel.Low)]);
            return Task.FromResult(plan);
        }
    }

    private sealed class CapturingPlanner : IAgentPlanner
    {
        public AgentPlannerRequest? LastRequest { get; private set; }

        public Task<AgentActionPlan> PlanAsync(AgentPlannerRequest request)
        {
            LastRequest = request;
            var plan = new AgentActionPlan(
                AgentIntent.Automation,
                request.Input,
                []);
            return Task.FromResult(plan);
        }
    }

    private sealed class FakeReflectionService : IAgentReflectionService
    {
        public bool Called { get; private set; }

        public Task<AgentActionPlan> ReflectAsync(AgentActionPlan plan)
        {
            Called = true;
            return Task.FromResult(plan);
        }
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
