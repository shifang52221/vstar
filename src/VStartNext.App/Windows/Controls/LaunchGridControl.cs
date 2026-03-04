using System.Drawing;
using System.Windows.Forms;
using VStartNext.App.Styles;

namespace VStartNext.App.Windows.Controls;

public sealed class LaunchGridControl : UserControl
{
    private static readonly IReadOnlyList<string> PinnedApps =
    [
        "Chrome",
        "Explorer",
        "VS Code",
        "PowerShell",
        "Notion",
        "Figma"
    ];

    private static readonly IReadOnlyList<string> RecentApps =
    [
        "Windows Terminal",
        "WeChat",
        "Edge"
    ];

    private static readonly IReadOnlyList<string> Folders =
    [
        "Work",
        "Development",
        "Media"
    ];

    private static readonly IReadOnlyList<string> SectionTitles =
    [
        "Pinned",
        "Recent",
        "Folders"
    ];

    public LaunchGridControl(NeoThemeTokens? tokens = null)
    {
        var theme = tokens ?? NeoThemeTokens.Default();

        Dock = DockStyle.Fill;
        Margin = new Padding(6);
        BackColor = ParseColor(theme.PanelColor, Color.FromArgb(34, 36, 44));
        Padding = new Padding(12);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 6,
            ColumnCount = 1
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 132));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 96));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        root.Controls.Add(BuildSectionTitle("Pinned", theme), 0, 0);
        root.Controls.Add(BuildAppCards(PinnedApps, theme), 0, 1);
        root.Controls.Add(BuildSectionTitle("Recent", theme), 0, 2);
        root.Controls.Add(BuildAppChips(RecentApps, theme), 0, 3);
        root.Controls.Add(BuildSectionTitle("Folders", theme), 0, 4);
        root.Controls.Add(BuildFolderCards(Folders, theme), 0, 5);

        Controls.Add(root);
    }

    public int PinnedAppCountForTesting => PinnedApps.Count;

    public int FolderCountForTesting => Folders.Count;

    public IReadOnlyList<string> SectionTitlesForTesting => SectionTitles;

    private static Control BuildSectionTitle(string title, NeoThemeTokens tokens)
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            Text = title,
            ForeColor = ParseColor(tokens.TextPrimaryColor, Color.FromArgb(224, 232, 240)),
            Font = new Font("Segoe UI Semibold", 10f, FontStyle.Regular),
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    private static Control BuildAppCards(IEnumerable<string> names, NeoThemeTokens tokens)
    {
        var row = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            WrapContents = true,
            Padding = new Padding(0, 2, 0, 2)
        };

        foreach (var name in names)
        {
            row.Controls.Add(new Button
            {
                Width = 124,
                Height = 54,
                Margin = new Padding(0, 0, 10, 10),
                FlatStyle = FlatStyle.Flat,
                Text = name,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = ParseColor(tokens.TextPrimaryColor, Color.FromArgb(230, 236, 245)),
                BackColor = ParseColor(tokens.CommandBarColor, Color.FromArgb(28, 30, 38))
            });
        }

        return row;
    }

    private static Control BuildAppChips(IEnumerable<string> names, NeoThemeTokens tokens)
    {
        var row = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = true,
            Padding = new Padding(0, 4, 0, 0)
        };

        foreach (var name in names)
        {
            row.Controls.Add(new Label
            {
                AutoSize = true,
                Margin = new Padding(0, 0, 8, 8),
                Padding = new Padding(10, 6, 10, 6),
                Text = name,
                BackColor = ParseColor(tokens.CommandInputColor, Color.FromArgb(22, 23, 30)),
                ForeColor = ParseColor(tokens.TextSecondaryColor, Color.FromArgb(170, 185, 196))
            });
        }

        return row;
    }

    private static Control BuildFolderCards(IEnumerable<string> names, NeoThemeTokens tokens)
    {
        var row = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            WrapContents = true,
            Padding = new Padding(0, 2, 0, 2)
        };

        foreach (var name in names)
        {
            row.Controls.Add(new Panel
            {
                Width = 178,
                Height = 88,
                Margin = new Padding(0, 0, 12, 12),
                BackColor = ParseColor(tokens.CommandBarColor, Color.FromArgb(28, 30, 38)),
                BorderStyle = BorderStyle.FixedSingle,
                Controls =
                {
                    new Label
                    {
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Text = name,
                        Font = new Font("Segoe UI Semibold", 9f, FontStyle.Regular),
                        ForeColor = ParseColor(tokens.TextPrimaryColor, Color.FromArgb(225, 235, 243))
                    }
                }
            });
        }

        return row;
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
