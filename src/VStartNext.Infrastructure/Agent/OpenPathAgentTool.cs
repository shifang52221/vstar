using System.Diagnostics;
using VStartNext.Core.Agent;
using VStartNext.Infrastructure.Launch;

namespace VStartNext.Infrastructure.Agent;

public sealed class OpenPathAgentTool : IAgentTool
{
    private readonly IProcessStarter _processStarter;

    public OpenPathAgentTool(IProcessStarter? processStarter = null)
    {
        _processStarter = processStarter ?? new SystemProcessStarter();
    }

    public string Name => "open_path";

    public Task<AgentToolResult> ExecuteAsync(string arguments)
    {
        var path = (arguments ?? string.Empty).Trim();
        if (path.Length == 0)
        {
            return Task.FromResult(new AgentToolResult(false, "Path is empty"));
        }

        if (!File.Exists(path) && !Directory.Exists(path))
        {
            return Task.FromResult(new AgentToolResult(false, $"Path not found: {path}"));
        }

        try
        {
            _processStarter.Start(new ProcessStartInfo(path)
            {
                UseShellExecute = true
            });
            return Task.FromResult(new AgentToolResult(true, $"Opened path: {path}"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new AgentToolResult(false, ex.Message));
        }
    }
}
