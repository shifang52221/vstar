using System.Diagnostics;
using VStartNext.Core.Agent;
using VStartNext.Infrastructure.Launch;

namespace VStartNext.Infrastructure.Agent;

public sealed class QuickActionAgentTool : IAgentTool
{
    private readonly IProcessStarter _processStarter;

    public QuickActionAgentTool(IProcessStarter? processStarter = null)
    {
        _processStarter = processStarter ?? new SystemProcessStarter();
    }

    public string Name => "quick_action";

    public Task<AgentToolResult> ExecuteAsync(string arguments)
    {
        var action = NormalizeAction(arguments);
        if (action.Length == 0)
        {
            return Task.FromResult(new AgentToolResult(false, "Quick action is empty"));
        }

        if (!TryBuildStartInfo(action, out var startInfo))
        {
            return Task.FromResult(new AgentToolResult(
                false,
                $"Unsupported quick action: {arguments}"));
        }

        try
        {
            _processStarter.Start(startInfo);
            return Task.FromResult(new AgentToolResult(true, $"Executed quick action: {action}"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new AgentToolResult(false, ex.Message));
        }
    }

    private static string NormalizeAction(string? arguments)
    {
        return (arguments ?? string.Empty).Trim().ToLowerInvariant();
    }

    private static bool TryBuildStartInfo(string action, out ProcessStartInfo startInfo)
    {
        startInfo = new ProcessStartInfo
        {
            UseShellExecute = true
        };

        switch (action)
        {
            case "shutdown":
                startInfo.FileName = "shutdown";
                startInfo.Arguments = "/s /t 0";
                return true;
            case "restart":
                startInfo.FileName = "shutdown";
                startInfo.Arguments = "/r /t 0";
                return true;
            case "lock":
                startInfo.FileName = "rundll32.exe";
                startInfo.Arguments = "user32.dll,LockWorkStation";
                return true;
            case "open_settings":
                startInfo.FileName = "ms-settings:";
                return true;
            default:
                return false;
        }
    }
}
