using FluentAssertions;
using VStartNext.Core.Search;
using Xunit;

namespace VStartNext.Core.Tests.Search;

public class SearchRankingTests
{
    [Fact]
    public void Ranking_UsesRecencyFrequencyAndPinWeight()
    {
        var engine = new SearchEngine();

        var top = engine.Rank(
            "ch",
            [
                new SearchItem("Chrome", 10, DateTimeOffset.UtcNow, true),
                new SearchItem("Chromium", 50, DateTimeOffset.UtcNow.AddDays(-30), false)
            ]).First();

        top.Name.Should().Be("Chrome");
    }
}
