using System.Diagnostics;
using VStartNext.Core.Agent;
using VStartNext.Infrastructure.Launch;

namespace VStartNext.Infrastructure.Agent;

public sealed class LaunchAppAgentTool : IAgentTool
{
    private readonly IProcessStarter _processStarter;

    public LaunchAppAgentTool(IProcessStarter? processStarter = null)
    {
        _processStarter = processStarter ?? new SystemProcessStarter();
    }

    public string Name => "launch_app";

    public Task<AgentToolResult> ExecuteAsync(string arguments)
    {
        var target = (arguments ?? string.Empty).Trim();
        if (target.Length == 0)
        {
            return Task.FromResult(new AgentToolResult(false, "App name is empty"));
        }

        try
        {
            _processStarter.Start(new ProcessStartInfo(target)
            {
                UseShellExecute = true
            });
            return Task.FromResult(new AgentToolResult(true, $"Launched app: {target}"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new AgentToolResult(false, ex.Message));
        }
    }
}
