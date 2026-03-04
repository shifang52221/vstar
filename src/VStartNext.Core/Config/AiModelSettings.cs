namespace VStartNext.Core.Config;

public sealed class AiModelSettings
{
    public AiProviderKind Provider { get; init; } = AiProviderKind.OpenAiCompatible;
    public string BaseUrl { get; init; } = "https://api.openai.com/v1";
    public string EncryptedApiKey { get; init; } = string.Empty;
    public AiModelRouteSettings Route { get; init; } = new();
    public double Temperature { get; init; } = 0.2;
    public int MaxTokens { get; init; } = 2048;
    public int TimeoutSeconds { get; init; } = 30;
    public int RetryCount { get; init; } = 2;
}
