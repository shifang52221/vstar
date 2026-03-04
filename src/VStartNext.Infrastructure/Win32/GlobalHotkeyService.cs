using VStartNext.Core.Abstractions;

namespace VStartNext.Infrastructure.Win32;

public sealed class GlobalHotkeyService : IHotkeyService
{
    public const int WmHotkey = 0x0312;

    private readonly nint _windowHandle;
    private readonly IWin32HotkeyApi _api;
    private readonly int _hotkeyId;

    private Action? _callback;
    private bool _registered;

    public GlobalHotkeyService(nint windowHandle, IWin32HotkeyApi? api = null, int hotkeyId = 1)
    {
        _windowHandle = windowHandle;
        _api = api ?? new Win32HotkeyApi();
        _hotkeyId = hotkeyId;
    }

    public void Register(HotkeyBinding binding, Action callback)
    {
        var modifiers = ToNativeModifiers(binding.Modifiers);
        var virtualKey = (uint)binding.Key;
        var ok = _api.RegisterHotKey(_windowHandle, _hotkeyId, modifiers, virtualKey);
        if (!ok)
        {
            throw new InvalidOperationException("RegisterHotKey failed.");
        }

        _callback = callback;
        _registered = true;
    }

    public void Unregister()
    {
        if (_registered)
        {
            _api.UnregisterHotKey(_windowHandle, _hotkeyId);
        }

        _callback = null;
        _registered = false;
    }

    public bool TryHandleWindowMessage(int message, nuint wParam)
    {
        if (message != WmHotkey || wParam != (nuint)_hotkeyId)
        {
            return false;
        }

        _callback?.Invoke();
        return true;
    }

    public static uint ToNativeModifiers(HotkeyModifiers modifiers)
    {
        return (uint)modifiers;
    }
}
