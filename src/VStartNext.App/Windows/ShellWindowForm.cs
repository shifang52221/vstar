using System.Drawing;
using System.Windows.Forms;

namespace VStartNext.App.Windows;

public sealed class ShellWindowForm : Form, IShellWindow
{
    public ShellWindowForm()
    {
        Text = "VStart Next";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(420, 560);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;

        var label = new Label
        {
            Text = "VStart Next",
            Dock = DockStyle.Top,
            Height = 52,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 16, FontStyle.Bold)
        };
        Controls.Add(label);

        var hint = new Label
        {
            Text = "Press Alt+Space to toggle",
            Dock = DockStyle.Top,
            Height = 36,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 10, FontStyle.Regular)
        };
        Controls.Add(hint);
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
}
