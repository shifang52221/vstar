using FluentAssertions;
using VStartNext.App.ViewModels;
using Xunit;

namespace VStartNext.Core.Tests.UI;

public class LauncherViewModelTests
{
    [Fact]
    public void DefaultCategories_ContainBrowserGroup()
    {
        var vm = new LauncherViewModel();
        vm.Categories.Should().Contain(category => category.Name == "Browser");
    }
}
