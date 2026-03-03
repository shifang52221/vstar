using FluentAssertions;
using VStartNext.Core.Catalog;
using Xunit;

namespace VStartNext.Core.Tests.Catalog;

public class CatalogOrderingTests
{
    [Fact]
    public void PinnedItems_AppearBeforeUnpinned()
    {
        var service = new CatalogService();

        var items = service.Sort(
        [
            new CatalogItem("A", false, 0),
            new CatalogItem("B", true, 5)
        ]);

        items[0].Name.Should().Be("B");
    }
}
