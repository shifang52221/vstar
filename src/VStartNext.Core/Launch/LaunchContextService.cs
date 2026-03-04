namespace VStartNext.Core.Launch;

public sealed class LaunchContextService
{
    public IReadOnlyList<LaunchContextAction> GetActions(string targetPath)
    {
        var folder = Path.GetDirectoryName(targetPath) ?? targetPath;

        return
        [
            new LaunchContextAction(LaunchContextActionType.RunAsAdmin, "Run as admin", targetPath),
            new LaunchContextAction(LaunchContextActionType.OpenDirectory, "Open directory", folder),
            new LaunchContextAction(LaunchContextActionType.CopyPath, "Copy path", targetPath)
        ];
    }
}
