using FluentAssertions;
using VStartNext.App.Windows;
using Xunit;

namespace VStartNext.Core.Tests.UI;

public class ShellWindowControllerTests
{
    [Fact]
    public void ApplyVisibility_True_ShowsWindow()
    {
        var fake = new FakeShellWindow();
        var controller = new ShellWindowController(fake);

        controller.ApplyVisibility(true);

        fake.Visible.Should().BeTrue();
    }

    [Fact]
    public void ApplyVisibility_False_HidesWindow()
    {
        var fake = new FakeShellWindow { Visible = true };
        var controller = new ShellWindowController(fake);

        controller.ApplyVisibility(false);

        fake.Visible.Should().BeFalse();
    }

    private sealed class FakeShellWindow : IShellWindow
    {
        public bool Visible { get; set; }

        public void ShowShell()
        {
            Visible = true;
        }

        public void HideShell()
        {
            Visible = false;
        }
    }
}
