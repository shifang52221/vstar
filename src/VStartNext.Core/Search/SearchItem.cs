namespace VStartNext.Core.Search;

public enum SearchTimeAffinity
{
    None,
    Morning,
    Afternoon,
    Evening
}

public sealed record SearchItem(
    string Name,
    int Frequency,
    DateTimeOffset LastUsed,
    bool Pinned,
    SearchTimeAffinity TimeAffinity = SearchTimeAffinity.None);
