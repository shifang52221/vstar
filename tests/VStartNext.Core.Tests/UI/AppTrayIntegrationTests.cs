using FluentAssertions;
using Xunit;

namespace VStartNext.Core.Tests.UI;

public class AppTrayIntegrationTests
{
    [Fact]
    public void TrayToggle_ChangesShellVisibility()
    {
        var app = new VStartNext.App.App();
        app.Shell.IsVisible.Should().BeFalse();

        app.Tray.RequestToggle();

        app.Shell.IsVisible.Should().BeTrue();
    }
}
