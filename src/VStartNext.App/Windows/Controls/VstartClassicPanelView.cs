using System.Drawing;
using System.Windows.Forms;
using VStartNext.App.Styles;

namespace VStartNext.App.Windows.Controls;

public sealed class VstartClassicPanelView : UserControl
{
    private static readonly IReadOnlyList<string> AppCategories = ["Design Software", "Browsers"];
    private static readonly IReadOnlyList<string> AppItems =
    [
        "Chrome",
        "QtWeb",
        "360 Browser",
        "Edge",
        "Firefox",
        "Baidu",
        "Chromium"
    ];

    private static readonly IReadOnlyList<string> ToolGroups =
    [
        "Common Tools",
        "Web Related",
        "Entertainment",
        "Mobile Related",
        "Programming",
        "Direct Apps",
        "Intelligent Training"
    ];

    private readonly TextBox _searchInput;
    private bool _focusCommandRequestedForTesting;

    public VstartClassicPanelView(NeoThemeTokens? tokens = null)
    {
        var theme = tokens ?? NeoThemeTokens.Default();

        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(236, 236, 236);
        Padding = new Padding(0);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 8
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 180));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 152));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

        root.Controls.Add(BuildHeroPanel(theme), 0, 0);
        root.Controls.Add(BuildTopTabs(theme), 0, 1);
        root.Controls.Add(BuildCategoryRows(theme), 0, 2);
        root.Controls.Add(BuildAppGrid(theme), 0, 3);
        root.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(236, 236, 236) }, 0, 4);
        root.Controls.Add(BuildToolGroups(theme), 0, 5);

        var searchRow = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(10, 6, 10, 6),
            BackColor = Color.FromArgb(230, 230, 230)
        };
        _searchInput = new TextBox
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle,
            PlaceholderText = "Search app or command"
        };
        _searchInput.KeyDown += SearchInputOnKeyDown;
        var searchButton = new Button
        {
            Dock = DockStyle.Right,
            Width = 38,
            Text = "Q",
            FlatStyle = FlatStyle.Flat,
            BackColor = ParseColor(theme.AccentColor, Color.FromArgb(51, 170, 72)),
            ForeColor = Color.White
        };
        searchButton.FlatAppearance.BorderSize = 0;
        searchButton.Click += (_, _) => CommandSubmitted?.Invoke(this, _searchInput.Text);
        searchRow.Controls.Add(_searchInput);
        searchRow.Controls.Add(searchButton);
        root.Controls.Add(searchRow, 0, 6);

        root.Controls.Add(BuildBottomBar(theme), 0, 7);

        Controls.Add(root);
    }

    public event EventHandler<string>? CommandSubmitted;

    public event EventHandler? AiSettingsRequested;

    public bool FocusCommandRequestedForTesting => _focusCommandRequestedForTesting;

    public int LauncherCategoryCountForTesting => AppCategories.Count;

    public void SubmitCommandForTesting(string input)
    {
        _searchInput.Text = input;
        CommandSubmitted?.Invoke(this, _searchInput.Text);
    }

    public void TriggerAiSettingsForTesting()
    {
        AiSettingsRequested?.Invoke(this, EventArgs.Empty);
    }

    public void FocusCommandInput()
    {
        _focusCommandRequestedForTesting = true;
        _searchInput.Focus();
    }

    private void SearchInputOnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
        {
            return;
        }

        e.SuppressKeyPress = true;
        e.Handled = true;
        CommandSubmitted?.Invoke(this, _searchInput.Text);
    }

    private static Control BuildHeroPanel(NeoThemeTokens tokens)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = ParseColor(tokens.AccentColor, Color.FromArgb(51, 170, 72)),
            Padding = new Padding(12, 10, 12, 8)
        };

        panel.Controls.Add(new Label
        {
            Dock = DockStyle.Top,
            Height = 24,
            Text = "Weather  |  00:00",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9f, FontStyle.Regular)
        });

        panel.Controls.Add(new Label
        {
            Dock = DockStyle.Top,
            Height = 26,
            Text = "Vstart Next",
            ForeColor = Color.White,
            Font = new Font("Segoe UI Semibold", 14f, FontStyle.Italic)
        });

        return panel;
    }

    private Control BuildTopTabs(NeoThemeTokens tokens)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = ParseColor(tokens.AccentColor, Color.FromArgb(51, 170, 72))
        };

        var homeButton = new Button
        {
            Dock = DockStyle.Left,
            Width = 64,
            Text = "Home",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(66, 182, 84),
            ForeColor = Color.White
        };
        homeButton.FlatAppearance.BorderSize = 0;

        var settingsButton = new Button
        {
            Dock = DockStyle.Left,
            Width = 64,
            Text = "AI",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(54, 168, 72),
            ForeColor = Color.White
        };
        settingsButton.FlatAppearance.BorderSize = 0;
        settingsButton.Click += (_, _) => AiSettingsRequested?.Invoke(this, EventArgs.Empty);

        panel.Controls.Add(settingsButton);
        panel.Controls.Add(homeButton);
        return panel;
    }

    private static Control BuildCategoryRows(NeoThemeTokens tokens)
    {
        var rows = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = AppCategories.Count,
            Padding = new Padding(12, 8, 12, 8)
        };

        foreach (var category in AppCategories)
        {
            rows.RowStyles.Add(new RowStyle(SizeType.Absolute, 18));
            rows.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Text = category,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(192, 220, 192),
                ForeColor = ParseColor(tokens.AccentColor, Color.FromArgb(39, 133, 52)),
                Font = new Font("Segoe UI", 9f, FontStyle.Regular)
            });
        }

        return rows;
    }

    private static Control BuildAppGrid(NeoThemeTokens tokens)
    {
        var grid = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = true,
            Padding = new Padding(12, 6, 12, 6)
        };

        foreach (var app in AppItems)
        {
            var tile = new Panel
            {
                Width = 68,
                Height = 74,
                Margin = new Padding(0, 0, 8, 8),
                BackColor = Color.Transparent
            };
            tile.Controls.Add(new Label
            {
                Dock = DockStyle.Bottom,
                Height = 20,
                Text = app,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular)
            });
            tile.Controls.Add(new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ParseColor(tokens.CommandBarColor, Color.FromArgb(164, 198, 235)),
                Margin = new Padding(10),
                BorderStyle = BorderStyle.FixedSingle
            });

            grid.Controls.Add(tile);
        }

        return grid;
    }

    private static Control BuildToolGroups(NeoThemeTokens tokens)
    {
        var list = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = ToolGroups.Count,
            Padding = new Padding(10, 0, 10, 8)
        };

        foreach (var group in ToolGroups)
        {
            list.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
            list.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Text = group,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(192, 220, 192),
                ForeColor = ParseColor(tokens.AccentColor, Color.FromArgb(39, 133, 52)),
                Font = new Font("Segoe UI", 9f, FontStyle.Regular)
            });
        }

        return list;
    }

    private static Control BuildBottomBar(NeoThemeTokens tokens)
    {
        var bar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = false,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(10, 8, 10, 8),
            BackColor = ParseColor(tokens.AccentColor, Color.FromArgb(51, 170, 72))
        };

        foreach (var icon in new[] { "V", "[]", "O", "^", "+", "*", "o", "C" })
        {
            bar.Controls.Add(new Label
            {
                Width = 26,
                Height = 24,
                Text = icon,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 11f, FontStyle.Regular)
            });
        }

        return bar;
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
