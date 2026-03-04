using System.Drawing;
using System.Windows.Forms;

namespace VStartNext.App.Windows;

public sealed class ShellWindowForm : Form, IShellWindow
{
    public ShellWindowForm()
    {
        Text = "VStart Next";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(980, 620);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        MinimumSize = new Size(980, 620);
        BackColor = Color.FromArgb(16, 17, 22);

        Controls.Add(new NeoPanelView());
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
