using FluentAssertions;
using VStartNext.App.Windows;
using Xunit;

namespace VStartNext.Core.Tests.UI;

public class NeoPanelViewTests
{
    [Fact]
    public void NeoPanel_HasFiveZones()
    {
        var view = new NeoPanelView();
        view.ZoneCount.Should().Be(5);
    }
}
