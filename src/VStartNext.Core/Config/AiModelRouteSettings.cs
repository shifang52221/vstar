namespace VStartNext.Core.Config;

public sealed class AiModelRouteSettings
{
    public string ChatModel { get; init; } = "gpt-4.1-mini";
    public string PlannerModel { get; init; } = "gpt-4.1";
    public string ReflectionModel { get; init; } = "gpt-4.1-mini";
}
