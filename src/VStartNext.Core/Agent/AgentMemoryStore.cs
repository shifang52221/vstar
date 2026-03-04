namespace VStartNext.Core.Agent;

public sealed class AgentMemoryStore
{
    private readonly Dictionary<string, int> _toolFrequency = new(StringComparer.OrdinalIgnoreCase);

    public AgentMemoryProfile Profile => new(PreferredLanguage, new Dictionary<string, int>(_toolFrequency));

    public AgentLanguage PreferredLanguage { get; private set; } = AgentLanguage.Mixed;

    public void RecordRun(AgentLanguage language, string toolName)
    {
        PreferredLanguage = language;

        if (_toolFrequency.TryGetValue(toolName, out var count))
        {
            _toolFrequency[toolName] = count + 1;
            return;
        }

        _toolFrequency[toolName] = 1;
    }
}
