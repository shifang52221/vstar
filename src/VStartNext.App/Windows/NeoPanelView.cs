using System.Drawing;
using System.Windows.Forms;

namespace VStartNext.App.Windows;

public sealed class NeoPanelView : UserControl
{
    public int ZoneCount => 5;

    public NeoPanelView()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(20, 20, 26);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 3,
            Padding = new Padding(12)
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 72));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));

        var top = ZonePanel("COMMAND");
        root.SetColumnSpan(top, 3);
        root.Controls.Add(top, 0, 0);

        root.Controls.Add(ZonePanel("NAV"), 0, 1);
        root.Controls.Add(ZonePanel("GRID"), 1, 1);
        root.Controls.Add(ZonePanel("CONTEXT"), 2, 1);

        var bottom = ZonePanel("STATUS");
        root.SetColumnSpan(bottom, 3);
        root.Controls.Add(bottom, 0, 2);

        Controls.Add(root);
    }

    private static Panel ZonePanel(string text)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(34, 36, 44),
            Margin = new Padding(6)
        };

        var label = new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.FromArgb(160, 180, 190),
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };
        panel.Controls.Add(label);
        return panel;
    }
}
