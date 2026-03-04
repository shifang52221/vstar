using System.Drawing;
using System.Windows.Forms;

namespace VStartNext.App.Windows;

public sealed class ShellWindowForm : Form, IShellWindow
{
    private readonly NeoPanelView _neoPanel;

    public event EventHandler<string>? CommandSubmitted;

    public ShellWindowForm()
    {
        Text = "VStart Next";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(980, 620);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        MinimumSize = new Size(980, 620);
        BackColor = Color.FromArgb(16, 17, 22);

        _neoPanel = new NeoPanelView();
        _neoPanel.CommandSubmitted += (_, input) => CommandSubmitted?.Invoke(this, input);
        Controls.Add(_neoPanel);
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

    public void SubmitCommandForTesting(string input)
    {
        _neoPanel.SubmitCommandForTesting(input);
    }
}
