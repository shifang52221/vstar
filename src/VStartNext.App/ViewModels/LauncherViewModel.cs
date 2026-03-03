namespace VStartNext.App.ViewModels;

public sealed record CategoryVm(string Name);

public sealed class LauncherViewModel
{
    public IReadOnlyList<CategoryVm> Categories { get; } =
    [
        new("Browser"),
        new("Tools"),
        new("Office")
    ];
}
