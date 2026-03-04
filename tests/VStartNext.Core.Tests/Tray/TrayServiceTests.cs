using FluentAssertions;
using VStartNext.App.Services;
using Xunit;

namespace VStartNext.Core.Tests.Tray;

public class TrayServiceTests
{
    [Fact]
    public void RequestToggle_AfterInitialize_InvokesCallback()
    {
        var tray = new TrayService();
        var toggled = false;

        tray.Initialize(() => toggled = true);
        tray.RequestToggle();

        toggled.Should().BeTrue();
    }
}
