namespace VStartNext.Core.Launch;

public sealed record LaunchRequest(string Target);

public sealed record LaunchResult(bool Success, string? ErrorMessage);
