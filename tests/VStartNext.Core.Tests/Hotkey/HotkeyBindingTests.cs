using FluentAssertions;
using VStartNext.Core.Abstractions;
using Xunit;

namespace VStartNext.Core.Tests.Hotkey;

public class HotkeyBindingTests
{
    [Fact]
    public void DefaultBinding_IsAltSpace()
    {
        var binding = HotkeyBinding.Default;
        binding.Modifiers.Should().Be(HotkeyModifiers.Alt);
        binding.Key.Should().Be(HotkeyKey.Space);
    }
}
