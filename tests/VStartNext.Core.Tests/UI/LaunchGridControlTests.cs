using FluentAssertions;
using VStartNext.App.Windows.Controls;

namespace VStartNext.Core.Tests.UI;

public class LaunchGridControlTests
{
    [Fact]
    public void Constructor_ProvidesPinnedAppsAndFolders()
    {
        using var control = new LaunchGridControl();

        control.PinnedAppCountForTesting.Should().BeGreaterThanOrEqualTo(6);
        control.FolderCountForTesting.Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public void Constructor_UsesLauncherFriendlySections()
    {
        using var control = new LaunchGridControl();

        control.SectionTitlesForTesting.Should().Contain("Pinned");
        control.SectionTitlesForTesting.Should().Contain("Recent");
        control.SectionTitlesForTesting.Should().Contain("Folders");
    }
}
