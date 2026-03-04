using FluentAssertions;
using VStartNext.Core.Config;
using Xunit;

namespace VStartNext.Core.Tests.Config;

public class AppConfigModelSettingsTests
{
    [Fact]
    public void DefaultConfig_IncludesModelSettings()
    {
        var config = AppConfig.Default();

        config.ModelSettings.Should().NotBeNull();
        config.ModelSettings.Route.ChatModel.Should().NotBeNullOrWhiteSpace();
        config.ModelSettings.Route.PlannerModel.Should().NotBeNullOrWhiteSpace();
        config.ModelSettings.Route.ReflectionModel.Should().NotBeNullOrWhiteSpace();
    }
}
