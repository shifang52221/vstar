using System.Windows.Forms;

namespace VStartNext.App.Win32;

internal sealed class HotkeyMessageWindow : NativeWindow, IDisposable
{
    private readonly Func<int, nuint, bool> _messageHandler;

    public HotkeyMessageWindow(Func<int, nuint, bool> messageHandler)
    {
        _messageHandler = messageHandler;
        CreateHandle(new CreateParams());
    }

    protected override void WndProc(ref Message m)
    {
        var handled = _messageHandler(m.Msg, (nuint)m.WParam.ToInt64());
        if (!handled)
        {
            base.WndProc(ref m);
        }
    }

    public void Dispose()
    {
        DestroyHandle();
    }
}
