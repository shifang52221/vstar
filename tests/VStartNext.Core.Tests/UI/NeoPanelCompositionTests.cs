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
    }
}
