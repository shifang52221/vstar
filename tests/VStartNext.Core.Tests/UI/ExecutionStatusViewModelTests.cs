using FluentAssertions;
using VStartNext.App.Windows.Execution;

namespace VStartNext.Core.Tests.UI;

public class ExecutionStatusViewModelTests
{
    [Fact]
    public void Constructor_UsesPlanningPhaseAndZeroTokens()
    {
        var viewModel = new ExecutionStatusViewModel();

        viewModel.Phase.Should().Be(ExecutionStatusPhase.Planning);
        viewModel.StreamTokenCount.Should().Be(0);
        viewModel.PhaseText.Should().Be("Planning");
        viewModel.StreamTokenText.Should().Be("Tokens: 0");
    }

    [Fact]
    public void SetPhase_UpdatesPhaseText()
    {
        var viewModel = new ExecutionStatusViewModel();

        viewModel.SetPhase(ExecutionStatusPhase.Finalizing);

        viewModel.Phase.Should().Be(ExecutionStatusPhase.Finalizing);
        viewModel.PhaseText.Should().Be("Finalizing");
    }

    [Fact]
    public void AddStreamToken_CountsOnlyNonWhitespaceTokens()
    {
        var viewModel = new ExecutionStatusViewModel();

        viewModel.AddStreamToken("Plan ");
        viewModel.AddStreamToken("");
        viewModel.AddStreamToken(" ");
        viewModel.AddStreamToken("done");

        viewModel.StreamTokenCount.Should().Be(2);
        viewModel.StreamTokenText.Should().Be("Tokens: 2");
    }
}
