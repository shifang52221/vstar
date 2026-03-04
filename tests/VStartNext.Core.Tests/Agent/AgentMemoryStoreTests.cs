using FluentAssertions;
using VStartNext.Core.Agent;
using Xunit;

namespace VStartNext.Core.Tests.Agent;

public class AgentMemoryStoreTests
{
    [Fact]
    public void RecordRun_UpdatesPreferredLanguageAndToolFrequency()
    {
        var store = new AgentMemoryStore();

        store.RecordRun(AgentLanguage.Chinese, "launch_app");

        store.Profile.PreferredLanguage.Should().Be(AgentLanguage.Chinese);
        store.Profile.ToolFrequency["launch_app"].Should().Be(1);
    }
}
