using System.Diagnostics;
using VStartNext.Core.Launch;

namespace VStartNext.Infrastructure.Launch;

public sealed class LaunchExecutor
{
    private readonly IProcessStarter _processStarter;

    public LaunchExecutor(IProcessStarter? processStarter = null)
    {
        _processStarter = processStarter ?? new SystemProcessStarter();
    }

    public Task<LaunchResult> LaunchAsync(LaunchRequest request)
    {
        var target = request.Target.Trim();
        if (target.Length == 0)
        {
            return Task.FromResult(new LaunchResult(false, "Target is empty"));
        }

        if (IsWebUrl(target))
        {
            return TryStart(new ProcessStartInfo(target)
            {
                UseShellExecute = true
            });
        }

        if (!File.Exists(target))
        {
            return Task.FromResult(new LaunchResult(false, "Target not found"));
        }

        var extension = Path.GetExtension(target).ToLowerInvariant();
        if (extension is ".exe" or ".lnk")
        {
            return TryStart(new ProcessStartInfo(target)
            {
                UseShellExecute = true
            });
        }

        return TryStart(new ProcessStartInfo(target)
        {
            UseShellExecute = true
        });
    }

    private Task<LaunchResult> TryStart(ProcessStartInfo startInfo)
    {
        try
        {
            _processStarter.Start(startInfo);
            return Task.FromResult(new LaunchResult(true, null));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new LaunchResult(false, ex.Message));
        }
    }

    private static bool IsWebUrl(string target)
    {
        return Uri.TryCreate(target, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
