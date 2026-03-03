using FluentAssertions;
using VStartNext.Infrastructure.Storage;
using Xunit;

namespace VStartNext.Core.Tests.Config;

public class ConfigRecoveryTests
{
    [Fact]
    public void CorruptedConfig_FallsBackToDefault()
    {
        var json = "{ broken";

        var config = JsonConfigStore.ParseOrDefault(json, out var recovered);

        config.SchemaVersion.Should().Be(1);
        recovered.Should().BeTrue();
    }
}
