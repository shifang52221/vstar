using VStartNext.Core.Config;
using VStartNext.Infrastructure.AI;
using VStartNext.Infrastructure.Security;
using VStartNext.Infrastructure.Storage;

namespace VStartNext.App.Settings;

public sealed class ModelSettingsService : IModelSettingsService
{
    private readonly AppConfigFileStore _configStore;
    private readonly ISecretProtector _secretProtector;
    private readonly IModelConnectionTester _connectionTester;

    public ModelSettingsService(
        AppConfigFileStore configStore,
        ISecretProtector secretProtector,
        IModelConnectionTester connectionTester)
    {
        _configStore = configStore;
        _secretProtector = secretProtector;
        _connectionTester = connectionTester;
    }

    public ModelSettingsInput Load()
    {
        var config = _configStore.Load();
        var model = config.ModelSettings;
        var apiKey = string.Empty;

        if (!string.IsNullOrWhiteSpace(model.EncryptedApiKey))
        {
            try
            {
                apiKey = _secretProtector.Unprotect(model.EncryptedApiKey);
            }
            catch
            {
                apiKey = string.Empty;
            }
        }

        return new ModelSettingsInput
        {
            Provider = model.Provider.ToString(),
            BaseUrl = model.BaseUrl,
            ApiKey = apiKey,
            ChatModel = model.Route.ChatModel,
            PlannerModel = model.Route.PlannerModel,
            ReflectionModel = model.Route.ReflectionModel,
            Temperature = model.Temperature,
            MaxTokens = model.MaxTokens,
            TimeoutSeconds = model.TimeoutSeconds,
            RetryCount = model.RetryCount
        };
    }

    public void Save(ModelSettingsInput input)
    {
        var existing = _configStore.Load();
        var encryptedApiKey = string.IsNullOrWhiteSpace(input.ApiKey)
            ? string.Empty
            : _secretProtector.Protect(input.ApiKey);

        var provider = Enum.TryParse<AiProviderKind>(input.Provider, out var parsed)
            ? parsed
            : AiProviderKind.OpenAiCompatible;

        var config = new AppConfig
        {
            SchemaVersion = existing.SchemaVersion,
            ModelSettings = new AiModelSettings
            {
                Provider = provider,
                BaseUrl = input.BaseUrl.Trim(),
                EncryptedApiKey = encryptedApiKey,
                Route = new AiModelRouteSettings
                {
                    ChatModel = input.ChatModel.Trim(),
                    PlannerModel = input.PlannerModel.Trim(),
                    ReflectionModel = input.ReflectionModel.Trim()
                },
                Temperature = input.Temperature,
                MaxTokens = input.MaxTokens,
                TimeoutSeconds = input.TimeoutSeconds,
                RetryCount = input.RetryCount
            }
        };

        _configStore.Save(config);
    }

    public Task<ModelConnectionTestResult> TestConnectionAsync(
        ModelSettingsInput input,
        CancellationToken cancellationToken = default)
    {
        var request = new ModelConnectionTestRequest(input.BaseUrl, input.ApiKey);
        return _connectionTester.TestAsync(request, cancellationToken);
    }
}
