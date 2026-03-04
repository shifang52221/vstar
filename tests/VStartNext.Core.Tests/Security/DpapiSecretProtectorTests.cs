using FluentAssertions;
using VStartNext.Infrastructure.Security;
using Xunit;

namespace VStartNext.Core.Tests.Security;

public class DpapiSecretProtectorTests
{
    [Fact]
    public void ProtectAndUnprotect_RoundTrip()
    {
        var protector = new DpapiSecretProtector();
        const string secret = "sk-test-secret";

        var encrypted = protector.Protect(secret);
        var plain = protector.Unprotect(encrypted);

        encrypted.Should().NotBe(secret);
        plain.Should().Be(secret);
    }
}
