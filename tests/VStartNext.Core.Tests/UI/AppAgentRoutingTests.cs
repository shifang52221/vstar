using FluentAssertions;
using VStartNext.App.Agent;
using VStartNext.Core.Search;
using Xunit;

namespace VStartNext.Core.Tests.UI;

public class AppAgentRoutingTests
{
    [Fact]
    public async Task HandleCommandInputAsync_NaturalLanguage_RoutesToAgent()
    {
        var app = new VStartNext.App.App(
            enableSystemTrayIcon: false,
            agentGateway: new FakeAgentGateway("done"));

        var result = await app.HandleCommandInputAsync("打开 chrome 并打开 github");

        result.DisplayText.Should().Be("done");
    }

    private sealed class FakeAgentGateway : IAppAgentGateway
    {
        private readonly string _displayText;

        public FakeAgentGateway(string displayText)
        {
            _displayText = displayText;
        }

        public bool ShouldHandle(string input)
        {
            return true;
        }

        public Task<CommandExecutionResult> ExecuteAsync(string input)
        {
            return Task.FromResult(new CommandExecutionResult(true, _displayText));
        }
    }
}
