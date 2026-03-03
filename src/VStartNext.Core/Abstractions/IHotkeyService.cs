namespace VStartNext.Core.Abstractions;

[Flags]
public enum HotkeyModifiers
{
    None = 0,
    Alt = 1,
    Ctrl = 2,
    Shift = 4,
    Win = 8
}

public enum HotkeyKey
{
    Space = 32
}

public readonly record struct HotkeyBinding(HotkeyModifiers Modifiers, HotkeyKey Key)
{
    public static HotkeyBinding Default => new(HotkeyModifiers.Alt, HotkeyKey.Space);
}

public interface IHotkeyService
{
    void Register(HotkeyBinding binding, Action callback);
    void Unregister();
}
