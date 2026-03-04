using System.Drawing;
using System.Windows.Forms;

namespace VStartNext.App.Windows.Controls;

public sealed class ContextPanelControl : UserControl
{
    public ContextPanelControl()
    {
        Dock = DockStyle.Fill;
        Margin = new Padding(6);
        BackColor = Color.FromArgb(32, 35, 42);

        Controls.Add(SectionLabel("Quick Actions"));
        Controls.Add(SectionLabel("Recent"));
    }

    private static Control SectionLabel(string text)
    {
        return new Label
        {
            Dock = DockStyle.Top,
            Height = 34,
            Text = text,
            ForeColor = Color.FromArgb(200, 205, 215),
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(8, 0, 0, 0)
        };
    }
}
