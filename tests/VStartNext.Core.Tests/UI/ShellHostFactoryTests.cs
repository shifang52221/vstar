using FluentAssertions;
using VStartNext.App.Windows;

namespace VStartNext.Core.Tests.UI;

public class ShellHostFactoryTests
{
    [Fact]
    public void ResolveMode_DefaultsToWinForms_WhenValueMissing()
    {
        var mode = ShellHostFactory.ResolveMode(null);

        mode.Should().Be(ShellHostMode.WinForms);
    }

    [Fact]
    public void ResolveMode_UsesWinUiPreview_WhenConfigured()
    {
        var mode = ShellHostFactory.ResolveMode("winui-preview");

        mode.Should().Be(ShellHostMode.WinUiPreview);
    }

    [Fact]
    public void ResolveMode_FallsBackToWinForms_WhenValueInvalid()
    {
        var mode = ShellHostFactory.ResolveMode("unknown-mode");

        mode.Should().Be(ShellHostMode.WinForms);
    }

    [Fact]
    public void Create_WithWinFormsMode_ReturnsWinFormsShellHost()
    {
        using var host = ShellHostFactory.Create(ShellHostMode.WinForms);

        host.Should().BeOfType<WinFormsShellHost>();
    }

    [Fact]
    public void Create_WithWinUiPreviewMode_ReturnsWinUiPreviewShellHost()
    {
        using var host = ShellHostFactory.Create(ShellHostMode.WinUiPreview);

        host.Should().BeOfType<WinUiPreviewShellHost>();
    }
}
