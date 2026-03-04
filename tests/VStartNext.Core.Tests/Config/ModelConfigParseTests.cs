using FluentAssertions;
using VStartNext.Infrastructure.Storage;
using Xunit;

namespace VStartNext.Core.Tests.Config;

public class ModelConfigParseTests
{
    [Fact]
    public void Parse_ModelSettingsWithStringProvider_PreservesValues()
    {
        var json = """
                   {
                     "SchemaVersion": 1,
                     "ModelSettings": {
                       "Provider": "OpenAiCompatible",
                       "BaseUrl": "https://api.example.com/v1",
                       "EncryptedApiKey": "cipher",
                       "Route": {
                         "ChatModel": "gpt-x",
                         "PlannerModel": "gpt-p",
                         "ReflectionModel": "gpt-r"
                       },
                       "Temperature": 0.4,
                       "MaxTokens": 1000,
                       "TimeoutSeconds": 20,
                       "RetryCount": 3
                     }
                   }
                   """;

        var config = JsonConfigStore.ParseOrDefault(json, out var recovered);

        recovered.Should().BeFalse();
        config.ModelSettings.Provider.ToString().Should().Be("OpenAiCompatible");
        config.ModelSettings.BaseUrl.Should().Be("https://api.example.com/v1");
        config.ModelSettings.Route.ChatModel.Should().Be("gpt-x");
    }
}
