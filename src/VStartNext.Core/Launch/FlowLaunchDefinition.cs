namespace VStartNext.Core.Launch;

public sealed record FlowLaunchDefinition(string Name, IReadOnlyList<FlowLaunchStep> Steps);
