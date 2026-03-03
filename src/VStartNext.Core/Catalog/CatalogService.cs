namespace VStartNext.Core.Catalog;

public sealed class CatalogService
{
    public List<CatalogItem> Sort(IEnumerable<CatalogItem> items)
    {
        return items
            .OrderByDescending(item => item.Pinned)
            .ThenByDescending(item => item.Order)
            .ToList();
    }
}
