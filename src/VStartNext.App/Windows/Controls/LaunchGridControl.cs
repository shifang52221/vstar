using System.Drawing;
using System.Windows.Forms;

namespace VStartNext.App.Windows.Controls;

public sealed class LaunchGridControl : UserControl
{
    public LaunchGridControl()
    {
        Dock = DockStyle.Fill;
        Margin = new Padding(6);
        BackColor = Color.FromArgb(34, 36, 44);

        var label = new Label
        {
            Dock = DockStyle.Top,
            Height = 30,
            Text = "Launch Grid",
            ForeColor = Color.FromArgb(210, 215, 220),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(8, 0, 0, 0)
        };
        Controls.Add(label);

        var gridStub = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            AutoScroll = true
        };
        Controls.Add(gridStub);
    }
}
