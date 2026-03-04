using FluentAssertions;
using VStartNext.Core.Agent;
using Xunit;

namespace VStartNext.Core.Tests.Agent;

public class AgentResponseLanguagePolicyTests
{
    [Fact]
    public void Resolve_FollowsInputLanguageByDefault()
    {
        var policy = new AgentResponseLanguagePolicy();

        var result = policy.Resolve("open chrome", uiLanguage: "zh-CN", followUiLanguage: false);

        result.Should().Be(AgentLanguage.English);
    }
}
