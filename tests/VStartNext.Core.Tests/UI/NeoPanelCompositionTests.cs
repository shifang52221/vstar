using FluentAssertions;
using VStartNext.App.Windows;
using Xunit;

namespace VStartNext.Core.Tests.UI;

public class NeoPanelCompositionTests
{
    [Fact]
    public void NeoPanel_ContainsCommandBarGridAndContextPanel()
    {
        var view = new NeoPanelView();

        view.HasCommandBar.Should().BeTrue();
        view.HasLaunchGrid.Should().BeTrue();
        view.HasContextPanel.Should().BeTrue();
        view.HasAiSettingsEntry.Should().BeTrue();
    }

    [Fact]
    public void NeoPanel_AiSettingsRequest_BubblesFromContextPanel()
    {
        using var view = new NeoPanelView();
        var raised = false;
        view.AiSettingsRequested += (_, _) => raised = true;

        view.TriggerAiSettingsForTesting();

        raised.Should().BeTrue();
    }
}
