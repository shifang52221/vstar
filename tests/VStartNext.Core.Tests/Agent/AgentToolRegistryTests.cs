using FluentAssertions;
using VStartNext.Core.Agent;
using Xunit;

namespace VStartNext.Core.Tests.Agent;

public class AgentToolRegistryTests
{
    [Fact]
    public async Task Registry_ResolvesAndExecutesRegisteredTool()
    {
        var registry = new AgentToolRegistry([new FakeTool("open_url")]);

        var result = await registry.ExecuteAsync("open_url", "https://openai.com");

        result.Success.Should().BeTrue();
        result.Message.Should().Be("ok:https://openai.com");
    }

    private sealed class FakeTool : IAgentTool
    {
        public FakeTool(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public Task<AgentToolResult> ExecuteAsync(string arguments)
        {
            return Task.FromResult(new AgentToolResult(true, $"ok:{arguments}"));
        }
    }
}
