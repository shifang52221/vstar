namespace VStartNext.Core.Agent;

public interface IAgentModelRouter
{
    Task<string> CompleteAsync(string prompt);

    IAsyncEnumerable<string> StreamCompletionAsync(
        string prompt,
        CancellationToken cancellationToken = default);
}
