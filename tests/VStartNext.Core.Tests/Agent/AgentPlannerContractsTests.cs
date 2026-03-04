using FluentAssertions;
using VStartNext.Core.Agent;
using Xunit;

namespace VStartNext.Core.Tests.Agent;

public class AgentPlannerContractsTests
{
    [Fact]
    public void PlannerRequest_CarriesInputLanguageAndToolCatalog()
    {
        var request = new AgentPlannerRequest("open chrome", AgentLanguage.English, ["launch_app"]);

        request.Language.Should().Be(AgentLanguage.English);
        request.AvailableTools.Should().Contain("launch_app");
    }
}
