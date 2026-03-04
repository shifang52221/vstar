using FluentAssertions;
using Xunit;

namespace VStartNext.Core.Tests.UI;

public class AppShellVisibilityEventsTests
{
    [Fact]
    public void TrayToggle_RaisesVisibilityChangedEvent()
    {
        var app = new VStartNext.App.App(enableSystemTrayIcon: false);
        bool? lastVisibility = null;
        app.ShellVisibilityChanged += visible => lastVisibility = visible;

        app.Tray.RequestToggle();

        lastVisibility.Should().BeTrue();
    }
}
