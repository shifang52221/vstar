using FluentAssertions;
using VStartNext.App.Windows.Controls;
using Xunit;

namespace VStartNext.Core.Tests.UI;

public class ContextPanelControlTests
{
    [Fact]
    public void AiSettingsButton_Click_RaisesRequestedEvent()
    {
        using var control = new ContextPanelControl();
        var raised = false;
        control.AiSettingsRequested += (_, _) => raised = true;

        control.TriggerAiSettingsForTesting();

        raised.Should().BeTrue();
    }

    [Fact]
    public void Constructor_ProvidesQuickActionsAndRecentItems()
    {
        using var control = new ContextPanelControl();

        control.QuickActionCountForTesting.Should().BeGreaterThanOrEqualTo(3);
        control.RecentItemCountForTesting.Should().BeGreaterThanOrEqualTo(3);
        control.QuickActionTitlesForTesting.Should().Contain("Run as admin");
    }
}
