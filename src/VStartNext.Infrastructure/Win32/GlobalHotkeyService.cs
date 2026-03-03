using VStartNext.Core.Abstractions;

namespace VStartNext.Infrastructure.Win32;

public sealed class GlobalHotkeyService : IHotkeyService
{
    private Action? _callback;
    private HotkeyBinding _binding;

    public void Register(HotkeyBinding binding, Action callback)
    {
        _binding = binding;
        _callback = callback;
    }

    public void Unregister()
    {
        _binding = default;
        _callback = null;
    }
}
