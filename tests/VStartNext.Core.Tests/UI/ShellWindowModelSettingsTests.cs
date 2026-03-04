using FluentAssertions;
using VStartNext.App.Windows;
using Xunit;

namespace VStartNext.Core.Tests.UI;

public class ShellWindowModelSettingsTests
{
    [Fact]
    public void TriggerAiSettings_InvokesOpenCallback()
    {
        var invoked = false;
        using var shell = new ShellWindowForm(onOpenModelSettings: () => invoked = true);

        shell.TriggerAiSettingsForTesting();

        invoked.Should().BeTrue();
    }
}
