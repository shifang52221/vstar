namespace VStartNext.App.Windows.Execution;

public sealed class ExecutionStatusViewModel
{
    public ExecutionStatusPhase Phase { get; private set; } = ExecutionStatusPhase.Planning;

    public int StreamTokenCount { get; private set; }

    public string PhaseText => Phase.ToString();

    public string StreamTokenText => $"Tokens: {StreamTokenCount}";

    public void SetPhase(ExecutionStatusPhase phase)
    {
        Phase = phase;
    }

    public void AddStreamToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return;
        }

        StreamTokenCount++;
    }
}
