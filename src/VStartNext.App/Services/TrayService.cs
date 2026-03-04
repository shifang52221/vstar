namespace VStartNext.App.Services;

public sealed class TrayService
{
    private Action? _toggleRequested;

    public bool IsInitialized { get; private set; }

    public void Initialize(Action toggleRequested)
    {
        _toggleRequested = toggleRequested;
        IsInitialized = true;
    }

    public void RequestToggle()
    {
        if (!IsInitialized)
        {
            return;
        }

        _toggleRequested?.Invoke();
    }
}
