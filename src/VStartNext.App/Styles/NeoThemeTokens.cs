namespace VStartNext.App.Styles;

public sealed record NeoThemeTokens(
    string BackgroundColor,
    string AccentColor,
    int RadiusSmall,
    int RadiusLarge,
    int SpacingSm,
    int SpacingMd,
    int SpacingLg)
{
    public static NeoThemeTokens Default()
    {
        return new NeoThemeTokens(
            BackgroundColor: "#101116",
            AccentColor: "#33D17A",
            RadiusSmall: 12,
            RadiusLarge: 16,
            SpacingSm: 8,
            SpacingMd: 16,
            SpacingLg: 24);
    }
}
