using System.Diagnostics;
using FluentAssertions;
using VStartNext.Infrastructure.Agent;
using VStartNext.Infrastructure.Launch;
using Xunit;

namespace VStartNext.Core.Tests.Agent;

public class LauncherAgentToolTests
{
    [Fact]
    public async Task LaunchAppTool_UsesShellExecute()
    {
        var starter = new FakeProcessStarter();
        var tool = new LaunchAppAgentTool(starter);

        var result = await tool.ExecuteAsync("chrome");

        result.Success.Should().BeTrue();
        starter.LastStartInfo.Should().NotBeNull();
        starter.LastStartInfo!.UseShellExecute.Should().BeTrue();
        starter.LastStartInfo.FileName.Should().Be("chrome");
    }

    [Fact]
    public async Task OpenUrlTool_InvalidUrl_ReturnsFailure()
    {
        var tool = new OpenUrlAgentTool();

        var result = await tool.ExecuteAsync("not-a-url");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid URL");
    }

    [Fact]
    public async Task OpenPathTool_MissingPath_ReturnsFailure()
    {
        var tool = new OpenPathAgentTool();

        var result = await tool.ExecuteAsync(@"C:\not-exist\missing.file");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Path not found");
    }

    [Fact]
    public async Task QuickActionTool_Shutdown_UsesExpectedCommand()
    {
        var starter = new FakeProcessStarter();
        var tool = new QuickActionAgentTool(starter);

        var result = await tool.ExecuteAsync("shutdown");

        result.Success.Should().BeTrue();
        starter.LastStartInfo.Should().NotBeNull();
        starter.LastStartInfo!.FileName.Should().Be("shutdown");
        starter.LastStartInfo.Arguments.Should().Be("/s /t 0");
    }

    [Fact]
    public async Task QuickActionTool_InvalidAction_ReturnsFailure()
    {
        var tool = new QuickActionAgentTool();

        var result = await tool.ExecuteAsync("format_disk");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Unsupported quick action");
    }

    private sealed class FakeProcessStarter : IProcessStarter
    {
        public ProcessStartInfo? LastStartInfo { get; private set; }

        public void Start(ProcessStartInfo startInfo)
        {
            LastStartInfo = startInfo;
        }
    }
}
