using System.Text.Json;
using VStartNext.Core.Agent;

namespace VStartNext.Infrastructure.Agent;

public sealed class AgentAuditFileStore : IAgentAuditStore
{
    private readonly string _path;
    private readonly int _maxEntries;
    private readonly object _sync = new();

    public AgentAuditFileStore(string? path = null, int maxEntries = 100)
    {
        _path = path ?? GetDefaultPath();
        _maxEntries = Math.Max(1, maxEntries);
    }

    public void Append(AgentAuditEntry entry)
    {
        lock (_sync)
        {
            var items = LoadInternal().ToList();
            items.Add(entry);
            if (items.Count > _maxEntries)
            {
                items = items.Skip(items.Count - _maxEntries).ToList();
            }

            SaveInternal(items);
        }
    }

    public IReadOnlyList<AgentAuditEntry> LoadRecent()
    {
        lock (_sync)
        {
            return LoadInternal();
        }
    }

    private IReadOnlyList<AgentAuditEntry> LoadInternal()
    {
        if (!File.Exists(_path))
        {
            return [];
        }

        try
        {
            var json = File.ReadAllText(_path);
            var items = JsonSerializer.Deserialize<List<AgentAuditEntry>>(json);
            return items ?? [];
        }
        catch
        {
            return [];
        }
    }

    private void SaveInternal(IReadOnlyList<AgentAuditEntry> items)
    {
        var directory = Path.GetDirectoryName(_path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(items, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(_path, json);
    }

    private static string GetDefaultPath()
    {
        var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(root, "VStartNext", "agent-audit.json");
    }
}
