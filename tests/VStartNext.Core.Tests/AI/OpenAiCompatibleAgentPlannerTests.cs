using FluentAssertions;
using VStartNext.Core.Agent;
using VStartNext.Infrastructure.AI;
using Xunit;

namespace VStartNext.Core.Tests.AI;

public class OpenAiCompatibleAgentPlannerTests
{
    [Fact]
    public async Task PlanAsync_ParsesJsonToolPlan()
    {
        var planner = new OpenAiCompatibleAgentPlanner(new FakeRouter("""
            {"intent":"Automation","steps":[{"toolName":"open_url","arguments":"https://openai.com","riskLevel":"Low"}]}
            """));

        var result = await planner.PlanAsync(new AgentPlannerRequest(
            "open openai",
            AgentLanguage.English,
            ["open_url", "launch_app"]));

        result.Intent.Should().Be(AgentIntent.Automation);
        result.Steps.Should().HaveCount(1);
        result.Steps[0].ToolName.Should().Be("open_url");
        result.Steps[0].Arguments.Should().Be("https://openai.com");
    }

    [Fact]
    public async Task PlanAsync_WithMarkdownJson_ParsesSteps()
    {
        var planner = new OpenAiCompatibleAgentPlanner(new FakeRouter("""
            ```json
            {"intent":"Automation","steps":[{"toolName":"launch_app","arguments":"chrome","riskLevel":"Low"}]}
            ```
            """));

        var result = await planner.PlanAsync(new AgentPlannerRequest(
            "打开 chrome",
            AgentLanguage.Chinese,
            ["launch_app"]));

        result.Steps.Should().HaveCount(1);
        result.Steps[0].ToolName.Should().Be("launch_app");
        result.Steps[0].Arguments.Should().Be("chrome");
    }

    private sealed class FakeRouter : IAgentModelRouter
    {
        private readonly string _response;

        public FakeRouter(string response)
        {
            _response = response;
        }

        public Task<string> CompleteAsync(string prompt)
        {
            return Task.FromResult(_response);
        }
    }
}
