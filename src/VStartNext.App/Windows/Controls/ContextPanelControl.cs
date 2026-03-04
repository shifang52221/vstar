using System.Drawing;
using System.Windows.Forms;
using VStartNext.App.Styles;

namespace VStartNext.App.Windows.Controls;

public sealed class ContextPanelControl : UserControl
{
    private static readonly IReadOnlyList<string> QuickActions =
    [
        "Run as admin",
        "Open directory",
        "Copy path"
    ];

    private static readonly IReadOnlyList<string> RecentItems =
    [
        "open chrome",
        "calc: 2*36",
        "url: github.com"
    ];

    private readonly Button _aiSettingsButton;

    public event EventHandler? AiSettingsRequested;

    public ContextPanelControl(NeoThemeTokens? tokens = null)
    {
        var theme = tokens ?? NeoThemeTokens.Default();

        Dock = DockStyle.Fill;
        Margin = new Padding(6);
        BackColor = ParseColor(theme.PanelColor, Color.FromArgb(32, 35, 42));
        Padding = new Padding(10);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 7
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 108));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 132));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 16));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        root.Controls.Add(BuildSectionLabel("Quick Actions", theme), 0, 0);
        root.Controls.Add(BuildQuickActionButtons(theme), 0, 1);
        root.Controls.Add(BuildSectionLabel("Recent", theme), 0, 2);
        root.Controls.Add(BuildRecentList(theme), 0, 3);

        _aiSettingsButton = new Button
        {
            Dock = DockStyle.Fill,
            Height = 32,
            Text = "AI Settings",
            FlatStyle = FlatStyle.Flat,
            BackColor = ParseColor(theme.CommandBarColor, Color.FromArgb(44, 50, 61)),
            ForeColor = ParseColor(theme.TextPrimaryColor, Color.FromArgb(228, 233, 240))
        };
        _aiSettingsButton.FlatAppearance.BorderColor = ParseColor(theme.AccentColor, Color.FromArgb(70, 84, 102));
        _aiSettingsButton.Click += (_, _) => AiSettingsRequested?.Invoke(this, EventArgs.Empty);
        root.Controls.Add(_aiSettingsButton, 0, 5);

        Controls.Add(root);
    }

    public int QuickActionCountForTesting => QuickActions.Count;

    public int RecentItemCountForTesting => RecentItems.Count;

    public IReadOnlyList<string> QuickActionTitlesForTesting => QuickActions;

    public void TriggerAiSettingsForTesting()
    {
        AiSettingsRequested?.Invoke(this, EventArgs.Empty);
    }

    private static Control BuildSectionLabel(string text, NeoThemeTokens tokens)
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            Text = text,
            ForeColor = ParseColor(tokens.TextPrimaryColor, Color.FromArgb(200, 205, 215)),
            Font = new Font("Segoe UI Semibold", 9f, FontStyle.Regular),
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    private static Control BuildQuickActionButtons(NeoThemeTokens tokens)
    {
        var list = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = QuickActions.Count,
            ColumnCount = 1
        };

        foreach (var action in QuickActions)
        {
            list.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            var button = new Button
            {
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Text = action,
                BackColor = ParseColor(tokens.CommandInputColor, Color.FromArgb(40, 45, 56)),
                ForeColor = ParseColor(tokens.TextSecondaryColor, Color.FromArgb(214, 222, 232))
            };
            button.FlatAppearance.BorderColor = ParseColor(tokens.PanelColor, Color.FromArgb(70, 84, 102));
            list.Controls.Add(button);
        }

        return list;
    }

    private static Control BuildRecentList(NeoThemeTokens tokens)
    {
        var list = new ListBox
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = ParseColor(tokens.CommandInputColor, Color.FromArgb(26, 30, 38)),
            ForeColor = ParseColor(tokens.TextSecondaryColor, Color.FromArgb(210, 220, 229))
        };

        foreach (var item in RecentItems)
        {
            list.Items.Add(item);
        }

        return list;
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
