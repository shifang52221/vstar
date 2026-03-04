using FluentAssertions;
using VStartNext.App.Agent;
using VStartNext.Core.Agent;
using Xunit;

namespace VStartNext.Core.Tests.UI;

public class AppAgentGatewayTests
{
    [Fact]
    public async Task ExecuteAsync_WithRunnerResult_IncludesExecutionTrace()
    {
        var gateway = new AppAgentGateway(agentRunner: new FakeRunner(
            new AgentRunResult(
                true,
                "Completed",
                [
                    new AgentStepExecution("launch_app", "chrome", true, "ok:chrome"),
                    new AgentStepExecution("open_url", "https://openai.com", true, "ok:url")
                ])));

        var result = await gateway.ExecuteAsync("open chrome and openai");

        result.Success.Should().BeTrue();
        result.DisplayText.Should().Contain("1.");
        result.DisplayText.Should().Contain("launch_app(chrome)");
        result.DisplayText.Should().Contain("open_url(https://openai.com)");
    }

    [Fact]
    public async Task ExecuteAsync_WithChineseInput_UsesChineseSummaryTemplate()
    {
        var gateway = new AppAgentGateway(agentRunner: new FakeRunner(
            new AgentRunResult(
                true,
                "Completed",
                [new AgentStepExecution("launch_app", "chrome", true, "ok:chrome")])));

        var result = await gateway.ExecuteAsync("打开 chrome");

        result.Success.Should().BeTrue();
        result.DisplayText.Should().Contain("已完成");
    }

    [Fact]
    public async Task ExecuteAsync_WhenHighRiskRequiresConfirmation_AndUserApproves_RerunsWithAutoConfirm()
    {
        var runner = new ConfirmingRunner();
        var gateway = new AppAgentGateway(
            agentRunner: runner,
            confirmHighRiskAction: _ => true);

        var result = await gateway.ExecuteAsync("shutdown now");

        result.Success.Should().BeTrue();
        runner.Calls.Should().Be(2);
        runner.CallsWithAutoConfirm.Should().ContainInOrder(false, true);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHighRiskRequiresConfirmation_AndUserRejects_ReturnsCanceled()
    {
        var runner = new ConfirmingRunner();
        var gateway = new AppAgentGateway(
            agentRunner: runner,
            confirmHighRiskAction: _ => false);

        var result = await gateway.ExecuteAsync("shutdown now");

        result.Success.Should().BeFalse();
        result.DisplayText.ToLowerInvariant().Should().Contain("canceled");
    }

    [Fact]
    public async Task ExecuteAsync_WithAgentRunner_ReturnsExecutionSummary()
    {
        var gateway = new AppAgentGateway(agentRunner: new FakeRunner(
            new AgentRunResult(
                true,
                "Completed",
                [new AgentStepExecution("launch_app", "chrome", true, "ok:chrome")])));

        var result = await gateway.ExecuteAsync("open chrome");

        result.Success.Should().BeTrue();
        result.DisplayText.Should().Contain("launch_app");
    }

    [Fact]
    public async Task ExecuteAsync_WithAgentRunnerFailure_ReturnsFailure()
    {
        var gateway = new AppAgentGateway(agentRunner: new FakeRunner(
            new AgentRunResult(false, "tool failed", [])));

        var result = await gateway.ExecuteAsync("open chrome");

        result.Success.Should().BeFalse();
        result.DisplayText.Should().Contain("tool failed");
    }

    [Fact]
    public async Task ExecuteAsync_WithAgentRunner_UsesRunnerInsteadOfModelRouter()
    {
        var gateway = new AppAgentGateway(
            modelRouter: new ThrowingRouter(),
            agentRunner: new FakeRunner(new AgentRunResult(true, "Completed", [])));

        var result = await gateway.ExecuteAsync("open chrome");

        result.Success.Should().BeTrue();
        result.DisplayText.Should().Contain("Completed");
    }

    [Fact]
    public async Task ExecuteAsync_WithModelRouter_ReturnsCompletionText()
    {
        var gateway = new AppAgentGateway(new FakeRouter("done"));

        var result = await gateway.ExecuteAsync("open chrome");

        result.Success.Should().BeTrue();
        result.DisplayText.Should().Be("done");
    }

    [Fact]
    public async Task ExecuteAsync_WhenModelRouterThrows_ReturnsFailure()
    {
        var gateway = new AppAgentGateway(new ThrowingRouter());

        var result = await gateway.ExecuteAsync("open chrome");

        result.Success.Should().BeFalse();
        result.DisplayText.ToLowerInvariant().Should().Contain("failed");
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

    private sealed class ThrowingRouter : IAgentModelRouter
    {
        public Task<string> CompleteAsync(string prompt)
        {
            throw new InvalidOperationException("boom");
        }
    }

    private sealed class FakeRunner : IAgentRunner
    {
        private readonly AgentRunResult _result;

        public FakeRunner(AgentRunResult result)
        {
            _result = result;
        }

        public Task<AgentRunResult> RunAsync(string input, bool autoConfirmHighRisk = true)
        {
            return Task.FromResult(_result);
        }
    }

    private sealed class ConfirmingRunner : IAgentRunner
    {
        public int Calls { get; private set; }
        public List<bool> CallsWithAutoConfirm { get; } = [];

        public Task<AgentRunResult> RunAsync(string input, bool autoConfirmHighRisk = true)
        {
            Calls++;
            CallsWithAutoConfirm.Add(autoConfirmHighRisk);
            if (!autoConfirmHighRisk)
            {
                return Task.FromResult(new AgentRunResult(
                    false,
                    "Confirmation required for quick_action(shutdown)",
                    [],
                    RequiresUserConfirmation: true));
            }

            return Task.FromResult(new AgentRunResult(
                true,
                "Completed",
                [new AgentStepExecution("quick_action", "shutdown", true, "done")]));
        }
    }
}
