using FluentAssertions;
using VStartNext.Core.Search;
using Xunit;

namespace VStartNext.Core.Tests.Search;

public class SearchRankingTimeAffinityTests
{
    [Fact]
    public void Rank_PrefersItemWithMatchingTimeAffinity()
    {
        var clock = new FixedClock(new DateTimeOffset(2026, 3, 4, 9, 0, 0, TimeSpan.Zero));
        var engine = new SearchEngine(clock);

        var top = engine.Rank(
            "bro",
            [
                new SearchItem("MorningBrowser", 10, clock.UtcNow.AddDays(-1), false, SearchTimeAffinity.Morning),
                new SearchItem("NeutralBrowser", 10, clock.UtcNow.AddDays(-1), false, SearchTimeAffinity.None)
            ]).First();

        top.Name.Should().Be("MorningBrowser");
    }

    private sealed class FixedClock : IClock
    {
        public FixedClock(DateTimeOffset utcNow)
        {
            UtcNow = utcNow;
        }

        public DateTimeOffset UtcNow { get; }
    }
}
