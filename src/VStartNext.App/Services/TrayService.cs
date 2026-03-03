namespace VStartNext.App.Services;

public sealed class TrayService
{
    public bool IsInitialized { get; private set; }

    public void Initialize()
    {
        IsInitialized = true;
    }
}
