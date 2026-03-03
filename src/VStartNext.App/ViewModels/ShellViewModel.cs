using VStartNext.Core.Shell;

namespace VStartNext.App.ViewModels;

public sealed class ShellViewModel : IShellController
{
    public bool IsVisible { get; private set; }

    public void ToggleVisibility()
    {
        IsVisible = !IsVisible;
    }
}
