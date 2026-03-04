using System.Text.Json;
using System.Text.Json.Serialization;
using VStartNext.Core.Config;

namespace VStartNext.Infrastructure.Storage;

public sealed class AppConfigFileStore
{
    private static readonly JsonSerializerOptions WriteOptions = CreateWriteOptions();

    public AppConfigFileStore(string? configPath = null)
    {
        ConfigPath = configPath ?? GetDefaultConfigPath();
    }

    public string ConfigPath { get; }

    public AppConfig Load()
    {
        if (!File.Exists(ConfigPath))
        {
            return AppConfig.Default();
        }

        var json = File.ReadAllText(ConfigPath);
        return JsonConfigStore.ParseOrDefault(json, out _);
    }

    public void Save(AppConfig config)
    {
        var directory = Path.GetDirectoryName(ConfigPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(config, WriteOptions);
        File.WriteAllText(ConfigPath, json);
    }

    private static string GetDefaultConfigPath()
    {
        var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(root, "VStartNext", "config.json");
    }

    private static JsonSerializerOptions CreateWriteOptions()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
