using Xunit;

namespace VStartNext.Core.Tests;

public class SmokeTests
{
    [Fact]
    public void CoreAssembly_Loads()
    {
        Assert.True(typeof(SmokeTests).Assembly.FullName is not null);
    }
}
