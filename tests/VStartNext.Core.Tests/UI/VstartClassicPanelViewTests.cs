using FluentAssertions;
using VStartNext.App.Windows.Controls;

namespace VStartNext.Core.Tests.UI;

public class VstartClassicPanelViewTests
{
    [Fact]
    public void Constructor_BuildsPrimaryLayoutZones()
    {
        using var view = new VstartClassicPanelView();

        view.HasHeroZoneForTesting.Should().BeTrue();
        view.HasCategoryZoneForTesting.Should().BeTrue();
        view.HasAppGridZoneForTesting.Should().BeTrue();
        view.HasToolZoneForTesting.Should().BeTrue();
        view.HasSearchZoneForTesting.Should().BeTrue();
        view.HasBottomDockZoneForTesting.Should().BeTrue();
    }

    [Fact]
    public void Constructor_UsesCollectedApps_WhenCatalogProvided()
    {
        var catalog = new FakeCatalog(
        [
            new LauncherAppEntry("Chrome", @"C:\Apps\Chrome.lnk"),
            new LauncherAppEntry("Edge", @"C:\Apps\Edge.lnk")
        ]);

        using var view = new VstartClassicPanelView(appCatalog: catalog);

        view.CollectedAppCountForTesting.Should().Be(2);
        view.CollectedAppNamesForTesting.Should().Contain("Chrome");
        view.CollectedAppNamesForTesting.Should().Contain("Edge");
        view.AppSlotCountForTesting.Should().BeGreaterThanOrEqualTo(8);
    }

    private sealed class FakeCatalog : IAppCatalog
    {
        private readonly IReadOnlyList<LauncherAppEntry> _entries;

        public FakeCatalog(IReadOnlyList<LauncherAppEntry> entries)
        {
            _entries = entries;
        }

        public IReadOnlyList<LauncherAppEntry> Load(int maxCount = 24)
        {
            return _entries;
        }
    }
}
