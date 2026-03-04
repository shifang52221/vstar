using FluentAssertions;
using VStartNext.App.Windows;

namespace VStartNext.Core.Tests.UI;

public class WinUiPreviewShellHostTests
{
    [Fact]
    public void OwnerWindow_UsesDedicatedPreviewForm()
    {
        using var host = new WinUiPreviewShellHost();

        host.OwnerWindow.Should().BeOfType<WinUiPreviewShellForm>();
    }

    [Fact]
    public void CommandSubmitted_IsForwardedFromPreviewForm()
    {
        using var host = new WinUiPreviewShellHost();
        var captured = string.Empty;
        host.CommandSubmitted += (_, input) => captured = input;

        ((WinUiPreviewShellForm)host.OwnerWindow).SubmitCommandForTesting("open chrome");

        captured.Should().Be("open chrome");
    }

    [Fact]
    public void SetOpenModelSettingsHandler_IsInvokedByPreviewFormAction()
    {
        using var host = new WinUiPreviewShellHost();
        var invoked = false;
        host.SetOpenModelSettingsHandler(() => invoked = true);

        ((WinUiPreviewShellForm)host.OwnerWindow).TriggerAiSettingsForTesting();

        invoked.Should().BeTrue();
    }
}
