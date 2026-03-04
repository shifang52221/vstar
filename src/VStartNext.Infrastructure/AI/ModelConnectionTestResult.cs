namespace VStartNext.Infrastructure.AI;

public sealed record ModelConnectionTestResult(bool Success, int? StatusCode, string Message);
