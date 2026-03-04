using VStartNext.Core.Launch;

namespace VStartNext.Infrastructure.Launch;

public interface IFlowStepExecutor
{
    Task<LaunchResult> ExecuteAsync(LaunchRequest request);
}

public sealed class FlowStepExecutor : IFlowStepExecutor
{
    private readonly LaunchExecutor _launchExecutor;

    public FlowStepExecutor(LaunchExecutor launchExecutor)
    {
        _launchExecutor = launchExecutor;
    }

    public Task<LaunchResult> ExecuteAsync(LaunchRequest request)
    {
        return _launchExecutor.LaunchAsync(request);
    }
}

public sealed class FlowLaunchRunner
{
    private readonly IFlowStepExecutor _executor;

    public FlowLaunchRunner(IFlowStepExecutor executor)
    {
        _executor = executor;
    }

    public async Task RunAsync(FlowLaunchDefinition definition)
    {
        foreach (var step in definition.Steps)
        {
            await _executor.ExecuteAsync(new LaunchRequest(step.Target));
        }
    }
}
