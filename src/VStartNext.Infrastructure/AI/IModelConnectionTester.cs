namespace VStartNext.Infrastructure.AI;

public interface IModelConnectionTester
{
    Task<ModelConnectionTestResult> TestAsync(
        ModelConnectionTestRequest request,
        CancellationToken cancellationToken = default);
}
