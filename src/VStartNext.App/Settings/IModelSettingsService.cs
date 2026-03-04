using VStartNext.Infrastructure.AI;

namespace VStartNext.App.Settings;

public interface IModelSettingsService
{
    ModelSettingsInput Load();
    void Save(ModelSettingsInput input);
    Task<ModelConnectionTestResult> TestConnectionAsync(
        ModelSettingsInput input,
        CancellationToken cancellationToken = default);
}
