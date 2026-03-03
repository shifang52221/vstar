namespace VStartNext.Core.Config;

public sealed class AppConfig
{
    public int SchemaVersion { get; init; } = 1;

    public static AppConfig Default()
    {
        return new AppConfig { SchemaVersion = 1 };
    }
}
