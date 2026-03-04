using FluentAssertions;
using VStartNext.App.Windows;
using VStartNext.Core.Agent;
using Xunit;

namespace VStartNext.Core.Tests.UI;

public class AgentExecutionPreviewFormTests
{
    [Fact]
    public void Form_ExposesExecuteAllSingleStepAndCancelModes()
    {
        var preview = new AgentExecutionPreview(
            "open chrome",
            [new AgentPlanStep("launch_app", "chrome", AgentRiskLevel.Low)]);
        using var form = new AgentExecutionPreviewForm(preview);

        form.TriggerExecuteAllForTesting();
        form.SelectedMode.Should().Be(AgentExecutionMode.ExecuteAll);

        form.TriggerSingleStepForTesting();
        form.SelectedMode.Should().Be(AgentExecutionMode.ExecuteSingleStep);

        form.TriggerCancelForTesting();
        form.SelectedMode.Should().Be(AgentExecutionMode.Cancel);
    }
}
