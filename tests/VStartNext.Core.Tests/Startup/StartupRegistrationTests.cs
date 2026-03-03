using FluentAssertions;
using VStartNext.Infrastructure.Startup;
using Xunit;

namespace VStartNext.Core.Tests.Startup;

public class StartupRegistrationTests
{
    [Fact]
    public void RegistryPath_IsCurrentUserRunKey()
    {
        StartupRegistrationService.RunKeyPath.Should()
            .Be(@"Software\Microsoft\Windows\CurrentVersion\Run");
    }
}
