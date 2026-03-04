using FluentAssertions;
using VStartNext.App.Agent;
using VStartNext.Core.Agent;
using Xunit;

namespace VStartNext.Core.Tests.UI;

public class AppAgentGatewayTests
{
    [Fact]
    public async Task ExecuteAsync_WithModelRouter_ReturnsCompletionText()
    {
        var gateway = new AppAgentGateway(new FakeRouter("done"));

        var result = await gateway.ExecuteAsync("open chrome");

        result.Success.Should().BeTrue();
        result.DisplayText.Should().Be("done");
    }

    [Fact]
    public async Task ExecuteAsync_WhenModelRouterThrows_ReturnsFailure()
    {
        var gateway = new AppAgentGateway(new ThrowingRouter());

        var result = await gateway.ExecuteAsync("open chrome");

        result.Success.Should().BeFalse();
        result.DisplayText.ToLowerInvariant().Should().Contain("failed");
    }

    private sealed class FakeRouter : IAgentModelRouter
    {
        private readonly string _response;

        public FakeRouter(string response)
        {
            _response = response;
        }

        public Task<string> CompleteAsync(string prompt)
        {
            return Task.FromResult(_response);
        }
    }

    private sealed class ThrowingRouter : IAgentModelRouter
    {
        public Task<string> CompleteAsync(string prompt)
        {
            throw new InvalidOperationException("boom");
        }
    }
}
