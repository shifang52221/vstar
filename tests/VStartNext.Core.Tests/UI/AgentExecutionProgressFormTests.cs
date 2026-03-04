using FluentAssertions;
using VStartNext.App.Windows;
using VStartNext.Core.Agent;
using Xunit;

namespace VStartNext.Core.Tests.UI;

public class AgentExecutionProgressFormTests
{
    [Fact]
    public void TriggerCancelForTesting_SetsCancelRequested()
    {
        var preview = new AgentExecutionPreview(
            "open chrome",
            [new AgentPlanStep("launch_app", "chrome", AgentRiskLevel.Low)]);
        using var form = new AgentExecutionProgressForm(preview, (_, _) =>
            Task.FromResult(new AgentRunResult(true, "Completed", [])));

        form.CancelRequested.Should().BeFalse();
        form.TriggerCancelForTesting();
        form.CancelRequested.Should().BeTrue();
    }

    [Fact]
    public async Task RunForTestingAsync_AppendsModelAndToolStreams()
    {
        var preview = new AgentExecutionPreview(
            "open chrome",
            [new AgentPlanStep("launch_app", "chrome", AgentRiskLevel.Low)]);
        using var form = new AgentExecutionProgressForm(preview, (_, progress) =>
        {
            progress.Report(new AgentExecutionUpdate(1, 1, "launch_app", "chrome", "Running", string.Empty));
            progress.Report(new AgentExecutionUpdate(1, 1, "launch_app", "chrome", "Succeeded", "ok:chrome"));
            return Task.FromResult(new AgentRunResult(
                true,
                "Completed",
                [new AgentStepExecution("launch_app", "chrome", true, "ok:chrome")]));
        });

        await form.RunForTestingAsync();

        form.ToolUpdateCountForTesting.Should().Be(2);
        form.ModelLinesForTesting.Should().Contain(line => line.Contains("started", StringComparison.OrdinalIgnoreCase));
        form.ModelLinesForTesting.Should().Contain(line => line.Contains("running", StringComparison.OrdinalIgnoreCase));
        form.ModelLinesForTesting.Should().Contain(line => line.Contains("completed", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task RunForTestingAsync_WithPlanningAndFinalizingStreams_AppendsTokens()
    {
        var preview = new AgentExecutionPreview(
            "open chrome",
            [new AgentPlanStep("launch_app", "chrome", AgentRiskLevel.Low)]);
        using var form = new AgentExecutionProgressForm(
            preview,
            (_, _) => Task.FromResult(new AgentRunResult(true, "Completed", [])),
            planningTokenStream: _ => StreamTokens("Plan ", "ready"),
            finalizingTokenStream: (_, _) => StreamTokens("Done"));

        await form.RunForTestingAsync();

        form.ModelStreamTextForTesting.Should().Contain("Plan ready");
        form.ModelStreamTextForTesting.Should().Contain("Done");
    }

    private static async IAsyncEnumerable<string> StreamTokens(params string[] tokens)
    {
        foreach (var token in tokens)
        {
            yield return token;
        }

        await Task.CompletedTask;
    }
}
