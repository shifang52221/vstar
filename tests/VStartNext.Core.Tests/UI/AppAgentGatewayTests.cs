using FluentAssertions;
using VStartNext.App.Agent;
using VStartNext.Core.Agent;
using Xunit;

namespace VStartNext.Core.Tests.UI;

public class AppAgentGatewayTests
{
    [Fact]
    public async Task ExecuteAsync_WhenPreviewModeIsCancel_ReturnsCanceledAndSkipsExecution()
    {
        var runner = new PreviewAwareRunner();
        var gateway = new AppAgentGateway(
            agentRunner: runner,
            selectExecutionMode: _ => AgentExecutionMode.Cancel);

        var result = await gateway.ExecuteAsync("open chrome and openai");

        result.Success.Should().BeFalse();
        result.DisplayText.ToLowerInvariant().Should().Contain("canceled");
        runner.PreviewCalls.Should().Be(1);
        runner.RunCalls.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPreviewModeIsSingleStep_RunsOnlyOneStep()
    {
        var runner = new PreviewAwareRunner();
        var gateway = new AppAgentGateway(
            agentRunner: runner,
            selectExecutionMode: _ => AgentExecutionMode.ExecuteSingleStep);

        var result = await gateway.ExecuteAsync("open chrome and openai");

        result.Success.Should().BeTrue();
        runner.RunCalls.Should().Be(1);
        runner.LastMaxSteps.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_RecordsAuditEntry()
    {
        var runner = new PreviewAwareRunner();
        var auditStore = new FakeAuditStore();
        var gateway = new AppAgentGateway(
            agentRunner: runner,
            auditStore: auditStore);

        var result = await gateway.ExecuteAsync("open chrome");

        result.Success.Should().BeTrue();
        auditStore.Entries.Should().HaveCount(1);
        auditStore.Entries[0].Input.Should().Be("open chrome");
        auditStore.Entries[0].Success.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WithRunnerResult_IncludesExecutionTrace()
    {
        var gateway = new AppAgentGateway(agentRunner: new StaticRunner(
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
        var gateway = new AppAgentGateway(agentRunner: new StaticRunner(
            new AgentRunResult(
                true,
                "Completed",
                [new AgentStepExecution("launch_app", "chrome", true, "ok:chrome")])),
            uiLanguage: "zh-CN",
            followUiLanguage: false);

        var result = await gateway.ExecuteAsync("\u6253\u5f00 chrome");

        result.Success.Should().BeTrue();
        result.DisplayText.Should().Contain("\u5df2\u5b8c\u6210");
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
        runner.RunCalls.Should().Be(2);
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
        var gateway = new AppAgentGateway(agentRunner: new StaticRunner(
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
        var gateway = new AppAgentGateway(agentRunner: new StaticRunner(
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
            agentRunner: new StaticRunner(new AgentRunResult(true, "Completed", [])));

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

    private sealed class StaticRunner : IAgentRunner
    {
        private readonly AgentRunResult _result;

        public StaticRunner(AgentRunResult result)
        {
            _result = result;
        }

        public Task<AgentExecutionPreview> PreviewAsync(string input)
        {
            return Task.FromResult(new AgentExecutionPreview(
                input,
                [new AgentPlanStep("launch_app", "chrome", AgentRiskLevel.Low)]));
        }

        public Task<AgentRunResult> RunAsync(
            AgentExecutionPreview preview,
            bool autoConfirmHighRisk = true,
            int? maxSteps = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_result);
        }
    }

    private sealed class ConfirmingRunner : IAgentRunner
    {
        public int RunCalls { get; private set; }
        public List<bool> CallsWithAutoConfirm { get; } = [];

        public Task<AgentExecutionPreview> PreviewAsync(string input)
        {
            return Task.FromResult(new AgentExecutionPreview(
                input,
                [new AgentPlanStep("quick_action", "shutdown", AgentRiskLevel.High)]));
        }

        public Task<AgentRunResult> RunAsync(
            AgentExecutionPreview preview,
            bool autoConfirmHighRisk = true,
            int? maxSteps = null,
            CancellationToken cancellationToken = default)
        {
            RunCalls++;
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

    private sealed class PreviewAwareRunner : IAgentRunner
    {
        public int PreviewCalls { get; private set; }
        public int RunCalls { get; private set; }
        public int? LastMaxSteps { get; private set; }

        public Task<AgentExecutionPreview> PreviewAsync(string input)
        {
            PreviewCalls++;
            return Task.FromResult(new AgentExecutionPreview(
                input,
                [
                    new AgentPlanStep("launch_app", "chrome", AgentRiskLevel.Low),
                    new AgentPlanStep("open_url", "https://openai.com", AgentRiskLevel.Low)
                ]));
        }

        public Task<AgentRunResult> RunAsync(
            AgentExecutionPreview preview,
            bool autoConfirmHighRisk = true,
            int? maxSteps = null,
            CancellationToken cancellationToken = default)
        {
            RunCalls++;
            LastMaxSteps = maxSteps;
            var executions = preview.Steps
                .Take(maxSteps ?? preview.Steps.Count)
                .Select(step => new AgentStepExecution(step.ToolName, step.Arguments, true, "ok"))
                .ToArray();
            return Task.FromResult(new AgentRunResult(true, "Completed", executions));
        }
    }

    private sealed class FakeAuditStore : IAgentAuditStore
    {
        public List<AgentAuditEntry> Entries { get; } = [];

        public void Append(AgentAuditEntry entry)
        {
            Entries.Add(entry);
        }

        public IReadOnlyList<AgentAuditEntry> LoadRecent()
        {
            return Entries;
        }
    }
}
