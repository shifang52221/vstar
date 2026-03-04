using System.Drawing;
using System.Windows.Forms;

namespace VStartNext.App.Services;

public sealed class TrayService : IDisposable
{
    private Action? _toggleRequested;
    private NotifyIcon? _notifyIcon;

    public bool IsInitialized { get; private set; }

    public void Initialize(Action toggleRequested, bool createSystemTrayIcon = false)
    {
        if (IsInitialized)
        {
            return;
        }

        _toggleRequested = toggleRequested;
        if (createSystemTrayIcon)
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Text = "VStart Next",
                Visible = true
            };
            _notifyIcon.MouseClick += (_, args) =>
            {
                if (args.Button == MouseButtons.Left)
                {
                    RequestToggle();
                }
            };
        }

        IsInitialized = true;
    }

    public void RequestToggle()
    {
        if (!IsInitialized)
        {
            return;
        }

        _toggleRequested?.Invoke();
    }

    public void Dispose()
    {
        if (_notifyIcon is null)
        {
            return;
        }

        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _notifyIcon = null;
    }
}
