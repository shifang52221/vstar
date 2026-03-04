using FluentAssertions;
using VStartNext.App.Settings;
using VStartNext.Core.Config;
using VStartNext.Infrastructure.AI;
using VStartNext.Infrastructure.Security;
using VStartNext.Infrastructure.Storage;
using Xunit;

namespace VStartNext.Core.Tests.UI;

public class ModelSettingsServiceTests
{
    [Fact]
    public void Save_EncryptsApiKeyInPersistedConfig()
    {
        var path = Path.Combine(Path.GetTempPath(), $"vstartnext-{Guid.NewGuid():N}.json");
        try
        {
            var store = new AppConfigFileStore(path);
            var service = new ModelSettingsService(store, new FakeProtector(), new FakeConnectionTester());
            var input = new ModelSettingsInput
            {
                BaseUrl = "https://api.example.com/v1",
                ApiKey = "sk-test",
                ChatModel = "gpt-chat",
                PlannerModel = "gpt-plan",
                ReflectionModel = "gpt-ref"
            };

            service.Save(input);
            var loaded = store.Load();

            loaded.ModelSettings.EncryptedApiKey.Should().Be("enc:sk-test");
            loaded.ModelSettings.EncryptedApiKey.Should().NotBe("sk-test");
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public void Load_DecryptsApiKeyForEditor()
    {
        var path = Path.Combine(Path.GetTempPath(), $"vstartnext-{Guid.NewGuid():N}.json");
        try
        {
            var store = new AppConfigFileStore(path);
            store.Save(new AppConfig
            {
                SchemaVersion = 1,
                ModelSettings = new AiModelSettings
                {
                    BaseUrl = "https://api.example.com/v1",
                    EncryptedApiKey = "enc:secret-key",
                    Route = new AiModelRouteSettings
                    {
                        ChatModel = "gpt-chat",
                        PlannerModel = "gpt-plan",
                        ReflectionModel = "gpt-ref"
                    }
                }
            });
            var service = new ModelSettingsService(store, new FakeProtector(), new FakeConnectionTester());

            var input = service.Load();

            input.ApiKey.Should().Be("secret-key");
            input.BaseUrl.Should().Be("https://api.example.com/v1");
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    private sealed class FakeProtector : ISecretProtector
    {
        public string Protect(string plaintext)
        {
            return $"enc:{plaintext}";
        }

        public string Unprotect(string ciphertext)
        {
            return ciphertext.Replace("enc:", string.Empty, StringComparison.Ordinal);
        }
    }

    private sealed class FakeConnectionTester : IModelConnectionTester
    {
        public Task<ModelConnectionTestResult> TestAsync(
            ModelConnectionTestRequest request,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ModelConnectionTestResult(true, 200, "ok"));
        }
    }
}
