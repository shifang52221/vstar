namespace VStartNext.Infrastructure.Win32;

public interface IWin32HotkeyApi
{
    bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);
    bool UnregisterHotKey(nint hWnd, int id);
}
