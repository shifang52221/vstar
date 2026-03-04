using System.Drawing;
using System.Windows.Forms;
using VStartNext.App.Styles;
using VStartNext.App.Windows.Controls;

namespace VStartNext.App.Windows;

public sealed class NeoPanelView : UserControl
{
    private readonly CommandBarControl _commandBar;
    private readonly CategoryRailControl _categoryRail;
    private readonly LaunchGridControl _launchGrid;
    private readonly ContextPanelControl _contextPanel;
    private readonly Panel _statusStrip;
    private readonly NeoThemeTokens _tokens;
    private bool _focusCommandRequestedForTesting;

    public event EventHandler<string>? CommandSubmitted;
    public event EventHandler? AiSettingsRequested;
    public int ZoneCount => 5;
    public bool HasCommandBar => _commandBar is not null;
    public bool HasCategoryRail => _categoryRail is not null;
    public bool HasLaunchGrid => _launchGrid is not null;
    public bool HasContextPanel => _contextPanel is not null;
    public bool HasStatusStrip => _statusStrip is not null;
    public bool HasAiSettingsEntry => _contextPanel is not null;
    public NeoThemeTokens ThemeTokensForTesting => _tokens;
    public bool FocusCommandRequestedForTesting => _focusCommandRequestedForTesting;

    public NeoPanelView()
        : this(NeoThemeTokens.Default())
    {
    }

    public NeoPanelView(NeoThemeTokens tokens)
    {
        _tokens = tokens;
        Dock = DockStyle.Fill;
        BackColor = ParseColor(_tokens.BackgroundColor, Color.FromArgb(20, 20, 26));

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 3,
            Padding = new Padding(_tokens.SpacingMd)
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 196));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 266));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));

        _commandBar = new CommandBarControl(_tokens);
        _commandBar.CommandSubmitted += (_, input) => CommandSubmitted?.Invoke(this, input);
        root.SetColumnSpan(_commandBar, 3);
        root.Controls.Add(_commandBar, 0, 0);

        _categoryRail = new CategoryRailControl(_tokens);
        root.Controls.Add(_categoryRail, 0, 1);

        _launchGrid = new LaunchGridControl(_tokens);
        root.Controls.Add(_launchGrid, 1, 1);

        _contextPanel = new ContextPanelControl(_tokens);
        _contextPanel.AiSettingsRequested += (_, _) => AiSettingsRequested?.Invoke(this, EventArgs.Empty);
        root.Controls.Add(_contextPanel, 2, 1);

        _statusStrip = BuildStatusStrip(_tokens);
        root.SetColumnSpan(_statusStrip, 3);
        root.Controls.Add(_statusStrip, 0, 2);

        Controls.Add(root);
    }

    private static Panel BuildStatusStrip(NeoThemeTokens tokens)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = ParseColor(tokens.PanelColor, Color.FromArgb(34, 36, 44)),
            Margin = new Padding(6),
            Padding = new Padding(12, 0, 12, 0)
        };

        var left = new Label
        {
            Dock = DockStyle.Left,
            Width = 260,
            Text = "Ready | Launcher mode",
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = ParseColor(tokens.TextSecondaryColor, Color.FromArgb(160, 180, 190)),
            Font = new Font("Segoe UI", 8.5f, FontStyle.Regular)
        };
        var right = new Label
        {
            Dock = DockStyle.Right,
            Width = 260,
            Text = "Quick  |  Agent  |  Flow",
            TextAlign = ContentAlignment.MiddleRight,
            ForeColor = ParseColor(tokens.TextSecondaryColor, Color.FromArgb(160, 180, 190)),
            Font = new Font("Segoe UI Semibold", 8.5f, FontStyle.Regular)
        };
        panel.Controls.Add(right);
        panel.Controls.Add(left);
        return panel;
    }

    public void SubmitCommandForTesting(string input)
    {
        _commandBar.SubmitForTesting(input);
    }

    public void TriggerAiSettingsForTesting()
    {
        _contextPanel.TriggerAiSettingsForTesting();
    }

    public void FocusCommandInput()
    {
        _focusCommandRequestedForTesting = true;
        _commandBar.FocusInput();
    }

    private static Color ParseColor(string value, Color fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        try
        {
            return ColorTranslator.FromHtml(value);
        }
        catch
        {
            return fallback;
        }
    }
}
