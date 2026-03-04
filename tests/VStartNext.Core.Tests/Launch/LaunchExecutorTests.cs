using System.Diagnostics;
using FluentAssertions;
using VStartNext.Core.Launch;
using VStartNext.Infrastructure.Launch;
using Xunit;

namespace VStartNext.Core.Tests.Launch;

public class LaunchExecutorTests
{
    [Fact]
    public async Task UrlTarget_UsesShellExecute()
    {
        var starter = new FakeProcessStarter();
        var executor = new LaunchExecutor(starter);

        var result = await executor.LaunchAsync(new LaunchRequest("https://example.com"));

        result.Success.Should().BeTrue();
        starter.LastStartInfo.Should().NotBeNull();
        starter.LastStartInfo!.UseShellExecute.Should().BeTrue();
        starter.LastStartInfo.FileName.Should().Be("https://example.com");
    }

    [Fact]
    public async Task LnkTarget_WhenFileExists_StartsSuccessfully()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.lnk");
        await File.WriteAllTextAsync(tempPath, "stub");
        try
        {
            var starter = new FakeProcessStarter();
            var executor = new LaunchExecutor(starter);

            var result = await executor.LaunchAsync(new LaunchRequest(tempPath));

            result.Success.Should().BeTrue();
            starter.LastStartInfo.Should().NotBeNull();
            starter.LastStartInfo!.FileName.Should().Be(tempPath);
            starter.LastStartInfo.UseShellExecute.Should().BeTrue();
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    [Fact]
    public async Task MissingPath_ReturnsReadableError()
    {
        var executor = new LaunchExecutor();

        var result = await executor.LaunchAsync(new LaunchRequest(@"C:\nope\missing.exe"));

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
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
