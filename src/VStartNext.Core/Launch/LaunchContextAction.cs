namespace VStartNext.Core.Launch;

public enum LaunchContextActionType
{
    RunAsAdmin,
    OpenDirectory,
    CopyPath
}

public sealed record LaunchContextAction(
    LaunchContextActionType Type,
    string Label,
    string Payload);
