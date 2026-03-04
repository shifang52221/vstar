using FluentAssertions;
using VStartNext.Infrastructure.Win32;
using Xunit;

namespace VStartNext.Core.Tests.UI;

public class AppHotkeyIntegrationTests
{
    [Fact]
    public void HotkeyMessage_TogglesShellVisibility()
    {
        var fakeApi = new FakeHotkeyApi();
        var app = new VStartNext.App.App(enableSystemTrayIcon: false);
        app.InitializeHotkey(new nint(42), fakeApi);
        app.Shell.IsVisible.Should().BeFalse();

        var handled = app.HandleWindowMessage(GlobalHotkeyService.WmHotkey, (nuint)1);

        handled.Should().BeTrue();
        app.Shell.IsVisible.Should().BeTrue();
        fakeApi.RegisterCalled.Should().BeTrue();
    }

    private sealed class FakeHotkeyApi : IWin32HotkeyApi
    {
        public bool RegisterCalled { get; private set; }

        public bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk)
        {
            RegisterCalled = true;
            return true;
        }

        public bool UnregisterHotKey(nint hWnd, int id)
        {
            return true;
        }
    }
}
