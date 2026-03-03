namespace VStartNext.Core.Search;

public sealed class SearchEngine
{
    public IEnumerable<SearchItem> Rank(string query, IEnumerable<SearchItem> items)
    {
        var normalizedQuery = query.Trim().ToLowerInvariant();

        return items
            .Where(item => item.Name.ToLowerInvariant().Contains(normalizedQuery))
            .OrderByDescending(Score);
    }

    private static double Score(SearchItem item)
    {
        var recencyDays = (DateTimeOffset.UtcNow - item.LastUsed).TotalDays;
        var recencyWeight = Math.Max(0, 30 - recencyDays);
        var pinWeight = item.Pinned ? 100 : 0;
        return pinWeight + item.Frequency + recencyWeight;
    }
}
