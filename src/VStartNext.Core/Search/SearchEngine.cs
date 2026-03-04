namespace VStartNext.Core.Search;

public sealed class SearchEngine
{
    private readonly IClock _clock;

    public SearchEngine(IClock? clock = null)
    {
        _clock = clock ?? new SystemClock();
    }

    public IEnumerable<SearchItem> Rank(string query, IEnumerable<SearchItem> items)
    {
        var normalizedQuery = query.Trim().ToLowerInvariant();

        return items
            .Where(item => item.Name.ToLowerInvariant().Contains(normalizedQuery))
            .OrderByDescending(Score);
    }

    private double Score(SearchItem item)
    {
        var recencyDays = (_clock.UtcNow - item.LastUsed).TotalDays;
        var recencyWeight = Math.Max(0, 30 - recencyDays);
        var pinWeight = item.Pinned ? 100 : 0;
        var affinityWeight = GetAffinityWeight(item.TimeAffinity, _clock.UtcNow.Hour);
        return pinWeight + item.Frequency + recencyWeight + affinityWeight;
    }

    private static double GetAffinityWeight(SearchTimeAffinity affinity, int hour)
    {
        return affinity switch
        {
            SearchTimeAffinity.Morning when hour is >= 5 and < 12 => 20,
            SearchTimeAffinity.Afternoon when hour is >= 12 and < 18 => 20,
            SearchTimeAffinity.Evening when hour is >= 18 or < 5 => 20,
            _ => 0
        };
    }
}
