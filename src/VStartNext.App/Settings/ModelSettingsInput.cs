namespace VStartNext.App.Settings;

public sealed class ModelSettingsInput
{
    public string Provider { get; init; } = "OpenAiCompatible";
    public string BaseUrl { get; init; } = string.Empty;
    public string ApiKey { get; init; } = string.Empty;
    public string ChatModel { get; init; } = string.Empty;
    public string PlannerModel { get; init; } = string.Empty;
    public string ReflectionModel { get; init; } = string.Empty;
    public double Temperature { get; init; } = 0.2;
    public int MaxTokens { get; init; } = 2048;
    public int TimeoutSeconds { get; init; } = 30;
    public int RetryCount { get; init; } = 2;
}
