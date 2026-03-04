using FluentAssertions;
using VStartNext.App.Styles;
using Xunit;

namespace VStartNext.Core.Tests.UI;

public class NeoThemeTokensTests
{
    [Fact]
    public void DefaultTheme_HasRequiredTokens()
    {
        var tokens = NeoThemeTokens.Default();

        tokens.RadiusLarge.Should().Be(16);
        tokens.SpacingLg.Should().Be(24);
        tokens.HeaderColor.Should().Be("#121A26");
        tokens.TextPrimaryColor.Should().Be("#EAF0F8");
    }
}
