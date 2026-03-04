namespace VStartNext.Core.Agent;

public interface IAgentModelRouter
{
    Task<string> CompleteAsync(string prompt);
}
