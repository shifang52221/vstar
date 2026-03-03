using System.Text.Json;
using VStartNext.Core.Config;

namespace VStartNext.Infrastructure.Storage;

public static class JsonConfigStore
{
    public static AppConfig ParseOrDefault(string json, out bool recovered)
    {
        try
        {
            recovered = false;
            return JsonSerializer.Deserialize<AppConfig>(json) ?? AppConfig.Default();
        }
        catch
        {
            recovered = true;
            return AppConfig.Default();
        }
    }
}
