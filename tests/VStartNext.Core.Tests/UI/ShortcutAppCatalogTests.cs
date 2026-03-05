using FluentAssertions;
using VStartNext.App.Windows.Controls;

namespace VStartNext.Core.Tests.UI;

public class ShortcutAppCatalogTests
{
    [Fact]
    public void Load_CollectsShortcutEntries_FromConfiguredRoots()
    {
        var root = Path.Combine(Path.GetTempPath(), $"vstart-next-shortcuts-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        var chrome = Path.Combine(root, "Chrome.lnk");
        var edge = Path.Combine(root, "Edge.url");
        File.WriteAllText(chrome, string.Empty);
        File.WriteAllText(edge, string.Empty);

        try
        {
            var catalog = new ShortcutAppCatalog([root]);

            var entries = catalog.Load(maxCount: 10);

            entries.Select(x => x.Name).Should().Contain("Chrome");
            entries.Select(x => x.Name).Should().Contain("Edge");
            entries.Should().OnlyContain(x => !string.IsNullOrWhiteSpace(x.TargetPath));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
