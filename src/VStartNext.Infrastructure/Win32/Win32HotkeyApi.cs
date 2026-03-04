using System.Runtime.InteropServices;

namespace VStartNext.Infrastructure.Win32;

public sealed class Win32HotkeyApi : IWin32HotkeyApi
{
    public bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk)
    {
        return RegisterHotKeyNative(hWnd, id, fsModifiers, vk);
    }

    public bool UnregisterHotKey(nint hWnd, int id)
    {
        return UnregisterHotKeyNative(hWnd, id);
    }

    [DllImport("user32.dll", EntryPoint = "RegisterHotKey", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RegisterHotKeyNative(nint hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", EntryPoint = "UnregisterHotKey", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnregisterHotKeyNative(nint hWnd, int id);
}
