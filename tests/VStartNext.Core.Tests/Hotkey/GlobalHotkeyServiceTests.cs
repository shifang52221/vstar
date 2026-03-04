using FluentAssertions;
using VStartNext.Core.Abstractions;
using VStartNext.Infrastructure.Win32;
using Xunit;

namespace VStartNext.Core.Tests.Hotkey;

public class GlobalHotkeyServiceTests
{
    [Fact]
    public void Register_UsesWin32ApiWithBindingValues()
    {
        var api = new FakeHotkeyApi();
        var sut = new GlobalHotkeyService(new nint(123), api);

        sut.Register(
            new HotkeyBinding(HotkeyModifiers.Alt | HotkeyModifiers.Ctrl, HotkeyKey.Space),
            () => { });

        api.RegisterCalled.Should().BeTrue();
        api.LastHwnd.Should().Be(new nint(123));
        api.LastModifiers.Should().Be(
            GlobalHotkeyService.ToNativeModifiers(HotkeyModifiers.Alt | HotkeyModifiers.Ctrl));
        api.LastVirtualKey.Should().Be((uint)HotkeyKey.Space);
    }

    [Fact]
    public void TryHandleWindowMessage_OnHotkey_InvokesCallback()
    {
        var api = new FakeHotkeyApi();
        var sut = new GlobalHotkeyService(new nint(123), api);
        var triggered = false;
        sut.Register(HotkeyBinding.Default, () => triggered = true);

        var handled = sut.TryHandleWindowMessage(0x0312, (nuint)1);

        handled.Should().BeTrue();
        triggered.Should().BeTrue();
    }

    private sealed class FakeHotkeyApi : IWin32HotkeyApi
    {
        public bool RegisterCalled { get; private set; }
        public nint LastHwnd { get; private set; }
        public uint LastModifiers { get; private set; }
        public uint LastVirtualKey { get; private set; }

        public bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk)
        {
            RegisterCalled = true;
            LastHwnd = hWnd;
            LastModifiers = fsModifiers;
            LastVirtualKey = vk;
            return true;
        }

        public bool UnregisterHotKey(nint hWnd, int id)
        {
            return true;
        }
    }
}
