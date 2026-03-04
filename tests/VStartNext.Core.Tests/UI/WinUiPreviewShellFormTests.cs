using System.Drawing;
using FluentAssertions;
using VStartNext.App.Styles;
using VStartNext.App.Windows;

namespace VStartNext.Core.Tests.UI;

public class WinUiPreviewShellFormTests
{
    [Fact]
    public void Constructor_AppliesThemeTokens_ToBackgroundAndHeader()
    {
        var tokens = NeoThemeTokens.Default();
        using var form = new WinUiPreviewShellForm(tokens);

        form.ThemeTokensForTesting.Should().Be(tokens);
        form.BackColor.Should().Be(ColorTranslator.FromHtml(tokens.BackgroundColor));
        form.HeaderBackColorForTesting.Should().Be(ColorTranslator.FromHtml(tokens.HeaderColor));
    }

    [Fact]
    public void TriggerHeaderInteractionForTesting_RequestsCommandFocus()
    {
        using var form = new WinUiPreviewShellForm(NeoThemeTokens.Default());

        form.TriggerHeaderInteractionForTesting();

        form.HeaderInteractionCountForTesting.Should().Be(1);
        form.CommandFocusRequestedForTesting.Should().BeTrue();
    }

    [Fact]
    public void Constructor_InitializesCloudAiBadgeText()
    {
        using var form = new WinUiPreviewShellForm(NeoThemeTokens.Default());

        form.AiBadgeTextForTesting.Should().Be("Cloud AI");
    }

    [Fact]
    public void SetModelProfileForTesting_UpdatesModelLabel()
    {
        using var form = new WinUiPreviewShellForm(NeoThemeTokens.Default());

        form.SetModelProfileForTesting("OpenAiCompatible", "gpt-4.1-mini");

        form.ModelProfileTextForTesting.Should().Be("OpenAiCompatible / gpt-4.1-mini");
    }

    [Fact]
    public void Constructor_UsesCompactWindowProfile_ForLauncherStyle()
    {
        using var form = new WinUiPreviewShellForm(NeoThemeTokens.Default());

        form.Width.Should().BeLessThanOrEqualTo(420);
        form.Height.Should().BeGreaterThanOrEqualTo(700);
    }

    [Fact]
    public void Constructor_UsesClassicLauncherSurface()
    {
        using var form = new WinUiPreviewShellForm(NeoThemeTokens.Default());

        form.UsesClassicLauncherSurfaceForTesting.Should().BeTrue();
        form.LauncherCategoryCountForTesting.Should().BeGreaterThanOrEqualTo(2);
    }
}
