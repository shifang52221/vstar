using FluentAssertions;
using System.Runtime.CompilerServices;
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

    [Fact]
    public async Task PlanAsync_WithInvalidSchema_FallsBackInsteadOfAcceptingInvalidPlan()
    {
        var planner = new OpenAiCompatibleAgentPlanner(new FakeRouter("""
            {"intent":"Automation","steps":[{"toolName":"launch_app","arguments":"chrome"}]}
            """));

        var result = await planner.PlanAsync(new AgentPlannerRequest(
            "hello there",
            AgentLanguage.English,
            ["launch_app"]));

        result.Intent.Should().Be(AgentIntent.Search);
        result.Steps.Should().BeEmpty();
    }

    [Fact]
    public async Task PlanAsync_WithPlanningProgress_StreamsPlannerTokens()
    {
        var router = new StreamingRouter(
            [
                "{\"intent\":\"Automation\",\"steps\":[",
                "{\"toolName\":\"launch_app\",\"arguments\":\"chrome\",\"riskLevel\":\"Low\"}",
                "]}"
            ]);
        var planner = new OpenAiCompatibleAgentPlanner(router);
        var progress = new CollectTextProgress();

        var result = await planner.PlanAsync(
            new AgentPlannerRequest("open chrome", AgentLanguage.English, ["launch_app"]),
            planningProgress: progress);

        result.Intent.Should().Be(AgentIntent.Automation);
        result.Steps.Should().HaveCount(1);
        progress.Items.Should().NotBeEmpty();
        progress.Items.Should().Contain(token => token.Contains("toolName", StringComparison.Ordinal));
        router.StreamCalls.Should().Be(1);
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

        public async IAsyncEnumerable<string> StreamCompletionAsync(string prompt, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield return _response;
            await Task.Yield();
        }
    }

    private sealed class StreamingRouter : IAgentModelRouter
    {
        private readonly IReadOnlyList<string> _tokens;

        public StreamingRouter(IReadOnlyList<string> tokens)
        {
            _tokens = tokens;
        }

        public int StreamCalls { get; private set; }

        public Task<string> CompleteAsync(string prompt)
        {
            return Task.FromResult(string.Concat(_tokens));
        }

        public async IAsyncEnumerable<string> StreamCompletionAsync(string prompt, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            StreamCalls++;
            foreach (var token in _tokens)
            {
                yield return token;
                await Task.CompletedTask;
            }
        }
    }

    private sealed class CollectTextProgress : IProgress<string>
    {
        public List<string> Items { get; } = [];

        public void Report(string value)
        {
            Items.Add(value);
        }
    }
}
