namespace VStartNext.Core.Search;

public sealed record SearchItem(
    string Name,
    int Frequency,
    DateTimeOffset LastUsed,
    bool Pinned);
