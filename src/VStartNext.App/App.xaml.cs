namespace VStartNext.App;

public partial class App
{
    public VStartNext.Core.Abstractions.HotkeyBinding StartupHotkey { get; } =
        VStartNext.Core.Abstractions.HotkeyBinding.Default;
}
