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
}
