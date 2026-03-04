using FluentAssertions;
using VStartNext.Core.Config;
using VStartNext.Infrastructure.Storage;
using Xunit;

namespace VStartNext.Core.Tests.Config;

public class AppConfigFileStoreTests
{
    [Fact]
    public void Load_WhenFileMissing_ReturnsDefault()
    {
        var path = Path.Combine(Path.GetTempPath(), $"vstartnext-{Guid.NewGuid():N}.json");
        var store = new AppConfigFileStore(path);

        var config = store.Load();

        config.Should().NotBeNull();
        config.SchemaVersion.Should().Be(1);
        config.ModelSettings.Should().NotBeNull();
    }

    [Fact]
    public void SaveThenLoad_RoundTripModelBaseUrl()
    {
        var path = Path.Combine(Path.GetTempPath(), $"vstartnext-{Guid.NewGuid():N}.json");
        try
        {
            var store = new AppConfigFileStore(path);
            var config = new AppConfig
            {
                ModelSettings = new AiModelSettings
                {
                    BaseUrl = "https://api.example.com/v1"
                }
            };

            store.Save(config);
            var loaded = store.Load();

            loaded.ModelSettings.BaseUrl.Should().Be("https://api.example.com/v1");
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
