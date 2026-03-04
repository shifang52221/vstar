using FluentAssertions;
using VStartNext.Core.Launch;
using VStartNext.Infrastructure.Launch;
using Xunit;

namespace VStartNext.Core.Tests.Launch;

public class FlowLaunchRunnerTests
{
    [Fact]
    public async Task Run_ExecutesStepsInOrder()
    {
        var fake = new FakeFlowStepExecutor();
        var runner = new FlowLaunchRunner(fake);
        var flow = new FlowLaunchDefinition(
            "Morning",
            [
                new FlowLaunchStep("browser.exe"),
                new FlowLaunchStep("https://a.com"),
                new FlowLaunchStep(@"C:\Work")
            ]);

        await runner.RunAsync(flow);

        fake.ExecutedTargets.Should().Equal("browser.exe", "https://a.com", @"C:\Work");
    }

    private sealed class FakeFlowStepExecutor : IFlowStepExecutor
    {
        public List<string> ExecutedTargets { get; } = [];

        public Task<LaunchResult> ExecuteAsync(LaunchRequest request)
        {
            ExecutedTargets.Add(request.Target);
            return Task.FromResult(new LaunchResult(true, null));
        }
    }
}
