using System.Drawing;
using System.Windows.Forms;
using VStartNext.App.Windows.Controls;

namespace VStartNext.App.Windows;

public sealed class NeoPanelView : UserControl
{
    private readonly CommandBarControl _commandBar;
    private readonly LaunchGridControl _launchGrid;
    private readonly ContextPanelControl _contextPanel;

    public event EventHandler<string>? CommandSubmitted;
    public int ZoneCount => 5;
    public bool HasCommandBar => _commandBar is not null;
    public bool HasLaunchGrid => _launchGrid is not null;
    public bool HasContextPanel => _contextPanel is not null;

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

        _commandBar = new CommandBarControl();
        _commandBar.CommandSubmitted += (_, input) => CommandSubmitted?.Invoke(this, input);
        root.SetColumnSpan(_commandBar, 3);
        root.Controls.Add(_commandBar, 0, 0);

        root.Controls.Add(ZonePanel("NAV"), 0, 1);
        _launchGrid = new LaunchGridControl();
        root.Controls.Add(_launchGrid, 1, 1);
        _contextPanel = new ContextPanelControl();
        root.Controls.Add(_contextPanel, 2, 1);

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

    public void SubmitCommandForTesting(string input)
    {
        _commandBar.SubmitForTesting(input);
    }
}
