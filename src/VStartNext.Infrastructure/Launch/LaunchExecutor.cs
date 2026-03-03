using VStartNext.Core.Launch;

namespace VStartNext.Infrastructure.Launch;

public sealed class LaunchExecutor
{
    public Task<LaunchResult> LaunchAsync(LaunchRequest request)
    {
        if (!File.Exists(request.Target))
        {
            return Task.FromResult(new LaunchResult(false, "Target not found"));
        }

        return Task.FromResult(new LaunchResult(true, null));
    }
}
