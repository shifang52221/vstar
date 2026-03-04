using FluentAssertions;
using VStartNext.Core.Launch;
using Xunit;

namespace VStartNext.Core.Tests.Launch;

public class LaunchContextServiceTests
{
    [Fact]
    public void GetActions_ProvidesRunAsAdminOpenDirCopyPath()
    {
        var service = new LaunchContextService();

        var actions = service.GetActions(@"C:\Tools\app.exe").Select(x => x.Type).ToList();

        actions.Should().Contain(LaunchContextActionType.RunAsAdmin);
        actions.Should().Contain(LaunchContextActionType.OpenDirectory);
        actions.Should().Contain(LaunchContextActionType.CopyPath);
    }
}
