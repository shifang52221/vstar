using FluentAssertions;
using VStartNext.Core.Launch;
using VStartNext.Infrastructure.Launch;
using Xunit;

namespace VStartNext.Core.Tests.Launch;

public class LaunchExecutorTests
{
    [Fact]
    public async Task MissingPath_ReturnsReadableError()
    {
        var executor = new LaunchExecutor();

        var result = await executor.LaunchAsync(new LaunchRequest(@"C:\nope\missing.exe"));

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }
}
