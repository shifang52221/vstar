using System.Drawing;
using System.Windows.Forms;

namespace VStartNext.App.Windows;

public sealed class WinUiPreviewShellForm : Form, IShellWindow
{
    private readonly NeoPanelView _neoPanel;
    private Action? _onOpenModelSettings;

    public event EventHandler<string>? CommandSubmitted;

    public WinUiPreviewShellForm(Action? onOpenModelSettings = null)
    {
        _onOpenModelSettings = onOpenModelSettings;
        Text = "VStart Next - WinUI Preview";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(1080, 680);
        MinimumSize = new Size(1080, 680);
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = false;
        BackColor = Color.FromArgb(12, 16, 24);

        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 44,
            BackColor = Color.FromArgb(18, 26, 38),
            Padding = new Padding(12, 10, 12, 8)
        };
        var headerTitle = new Label
        {
            Dock = DockStyle.Left,
            AutoSize = false,
            Width = 340,
            ForeColor = Color.FromArgb(230, 240, 248),
            Font = new Font("Segoe UI Semibold", 10f, FontStyle.Regular),
            Text = "WinUI Preview Shell"
        };
        header.Controls.Add(headerTitle);

        _neoPanel = new NeoPanelView();
        _neoPanel.Dock = DockStyle.Fill;
        _neoPanel.CommandSubmitted += (_, input) => CommandSubmitted?.Invoke(this, input);
        _neoPanel.AiSettingsRequested += (_, _) => _onOpenModelSettings?.Invoke();

        Controls.Add(_neoPanel);
        Controls.Add(header);
    }

    public void ShowShell()
    {
        if (!Visible)
        {
            Show();
        }

        Activate();
    }

    public void HideShell()
    {
        Hide();
    }

    public void SetOpenModelSettingsHandler(Action onOpenModelSettings)
    {
        _onOpenModelSettings = onOpenModelSettings;
    }

    public void SubmitCommandForTesting(string input)
    {
        _neoPanel.SubmitCommandForTesting(input);
    }

    public void TriggerAiSettingsForTesting()
    {
        _neoPanel.TriggerAiSettingsForTesting();
    }
}
