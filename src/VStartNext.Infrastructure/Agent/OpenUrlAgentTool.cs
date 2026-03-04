using System.Diagnostics;
using VStartNext.Core.Agent;
using VStartNext.Infrastructure.Launch;

namespace VStartNext.Infrastructure.Agent;

public sealed class OpenUrlAgentTool : IAgentTool
{
    private readonly IProcessStarter _processStarter;

    public OpenUrlAgentTool(IProcessStarter? processStarter = null)
    {
        _processStarter = processStarter ?? new SystemProcessStarter();
    }

    public string Name => "open_url";

    public Task<AgentToolResult> ExecuteAsync(string arguments)
    {
        var value = (arguments ?? string.Empty).Trim();
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return Task.FromResult(new AgentToolResult(false, "Invalid URL"));
        }

        try
        {
            _processStarter.Start(new ProcessStartInfo(uri.ToString())
            {
                UseShellExecute = true
            });
            return Task.FromResult(new AgentToolResult(true, $"Opened URL: {uri}"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new AgentToolResult(false, ex.Message));
        }
    }
}
