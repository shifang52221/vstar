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
    }
}
