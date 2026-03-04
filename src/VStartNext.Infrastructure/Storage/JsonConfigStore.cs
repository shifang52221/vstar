using System.Text.Json;
using System.Text.Json.Serialization;
using VStartNext.Core.Config;

namespace VStartNext.Infrastructure.Storage;

public static class JsonConfigStore
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    public static AppConfig ParseOrDefault(string json, out bool recovered)
    {
        try
        {
            recovered = false;
            return JsonSerializer.Deserialize<AppConfig>(json, JsonOptions) ?? AppConfig.Default();
        }
        catch
        {
            recovered = true;
            return AppConfig.Default();
        }
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
