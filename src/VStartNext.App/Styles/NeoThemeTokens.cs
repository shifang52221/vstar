namespace VStartNext.App.Styles;

public sealed record NeoThemeTokens(
    string BackgroundColor,
    string PanelColor,
    string HeaderColor,
    string AccentColor,
    string TextPrimaryColor,
    string TextSecondaryColor,
    string CommandBarColor,
    string CommandInputColor,
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
            PanelColor: "#22242C",
            HeaderColor: "#121A26",
            AccentColor: "#33D17A",
            TextPrimaryColor: "#EAF0F8",
            TextSecondaryColor: "#A0B4BE",
            CommandBarColor: "#1C1E26",
            CommandInputColor: "#16171E",
            RadiusSmall: 12,
            RadiusLarge: 16,
            SpacingSm: 8,
            SpacingMd: 16,
            SpacingLg: 24);
    }
}
