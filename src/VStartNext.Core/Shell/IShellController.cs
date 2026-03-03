namespace VStartNext.Core.Shell;

public interface IShellController
{
    bool IsVisible { get; }
    void ToggleVisibility();
}
