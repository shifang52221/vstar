using FluentAssertions;
using VStartNext.App.Windows;
using Xunit;

namespace VStartNext.Core.Tests.UI;

public class NeoPanelInteractionFlowTests
{
    [Fact]
    public async Task CommandBarInput_RoutesToPaletteAndUpdatesResultState()
    {
        var app = new VStartNext.App.App(enableSystemTrayIcon: false);

        var updated = await app.HandleCommandInputAsync("calc: 2*36");

        updated.DisplayText.Should().Be("72");
        app.LastCommandResult.Should().Be(updated);
    }

    [Fact]
    public async Task ShellCommandSubmission_RoutesThroughAppOrchestration()
    {
        var app = new VStartNext.App.App(enableSystemTrayIcon: false);
        using var shell = new ShellWindowForm();
        var completion = new TaskCompletionSource<string>();

        shell.CommandSubmitted += async (_, input) =>
        {
            var result = await app.HandleCommandInputAsync(input);
            completion.TrySetResult(result.DisplayText);
        };

        shell.SubmitCommandForTesting("calc: 3*7");
        var display = await completion.Task.WaitAsync(TimeSpan.FromSeconds(1));

        display.Should().Be("21");
        app.LastCommandResult?.DisplayText.Should().Be("21");
    }
}
