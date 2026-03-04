namespace VStartNext.Core.Config;

public sealed class AppConfig
{
    public int SchemaVersion { get; init; } = 1;
    public AiModelSettings ModelSettings { get; init; } = new();

    public static AppConfig Default()
    {
        return new AppConfig
        {
            SchemaVersion = 1,
            ModelSettings = new AiModelSettings()
        };
    }
}
