namespace VStartNext.Core.Agent;

public interface IAgentRunner
{
    Task<AgentExecutionPreview> PreviewAsync(string input);

    Task<AgentRunResult> RunAsync(
        AgentExecutionPreview preview,
        bool autoConfirmHighRisk = true,
        int? maxSteps = null,
        CancellationToken cancellationToken = default,
        IProgress<AgentExecutionUpdate>? progress = null);
}
