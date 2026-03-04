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
}
