using FluentAssertions;
using VStartNext.App.ViewModels;
using Xunit;

namespace VStartNext.Core.Tests.Shell;

public class ShellToggleTests
{
    [Fact]
    public void ToggleVisibility_FlipsState()
    {
        var vm = new ShellViewModel();
        vm.IsVisible.Should().BeFalse();

        vm.ToggleVisibility();

        vm.IsVisible.Should().BeTrue();
    }
}
