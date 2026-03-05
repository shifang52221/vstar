using System.Drawing;
using System.Windows.Forms;
using VStartNext.App.Styles;

namespace VStartNext.App.Windows.Controls;

public sealed class VstartClassicPanelView : UserControl
{
    private static readonly IReadOnlyList<string> AppCategories = ["Design Software", "Browsers"];
    private static readonly IReadOnlyList<LauncherAppEntry> FallbackAppItems =
    [
        new LauncherAppEntry("Chrome", string.Empty),
        new LauncherAppEntry("QtWeb", string.Empty),
        new LauncherAppEntry("360 Browser", string.Empty),
        new LauncherAppEntry("Edge", string.Empty),
        new LauncherAppEntry("Firefox", string.Empty),
        new LauncherAppEntry("Baidu", string.Empty),
        new LauncherAppEntry("Chromium", string.Empty),
        new LauncherAppEntry("Toolkit", string.Empty)
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
    private readonly FlowLayoutPanel _appGrid;
    private readonly Panel _heroZone;
    private readonly Panel _categoryZone;
    private readonly Panel _appGridZone;
    private readonly Panel _toolZone;
    private readonly Panel _searchZone;
    private readonly Panel _bottomDockZone;
    private readonly IReadOnlyList<LauncherAppEntry> _appItems;
    private bool _focusCommandRequestedForTesting;

    public VstartClassicPanelView(
        NeoThemeTokens? tokens = null,
        IAppCatalog? appCatalog = null)
    {
        var theme = tokens ?? NeoThemeTokens.Default();
        var catalog = appCatalog ?? new ShortcutAppCatalog();
        _appItems = catalog.Load(maxCount: 12);
        if (_appItems.Count == 0)
        {
            _appItems = FallbackAppItems;
        }

        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(236, 236, 236);
        Padding = new Padding(0);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 8
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 96));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 210));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 156));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));

        _heroZone = BuildHeroZone(theme);
        _categoryZone = BuildCategoryZone(theme);
        _appGrid = BuildAppGrid(theme, EnsureMinimumSlots(_appItems, minimumSlots: 8));
        _appGridZone = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(236, 236, 236) };
        _appGridZone.Controls.Add(_appGrid);
        _toolZone = BuildToolZone(theme);
        _searchZone = BuildSearchZone(theme, out _searchInput);
        _bottomDockZone = BuildBottomDockZone(theme);

        root.Controls.Add(_heroZone, 0, 0);
        root.Controls.Add(BuildTopTabs(theme), 0, 1);
        root.Controls.Add(_categoryZone, 0, 2);
        root.Controls.Add(_appGridZone, 0, 3);
        root.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(236, 236, 236) }, 0, 4);
        root.Controls.Add(_toolZone, 0, 5);
        root.Controls.Add(_searchZone, 0, 6);
        root.Controls.Add(_bottomDockZone, 0, 7);

        Controls.Add(root);
    }

    public event EventHandler<string>? CommandSubmitted;

    public event EventHandler? AiSettingsRequested;

    public bool FocusCommandRequestedForTesting => _focusCommandRequestedForTesting;

    public int LauncherCategoryCountForTesting => AppCategories.Count;

    public int CollectedAppCountForTesting => _appItems.Count;

    public IReadOnlyList<string> CollectedAppNamesForTesting => _appItems.Select(x => x.Name).ToArray();

    public bool HasHeroZoneForTesting => _heroZone is not null;

    public bool HasCategoryZoneForTesting => _categoryZone is not null;

    public bool HasAppGridZoneForTesting => _appGridZone is not null;

    public bool HasToolZoneForTesting => _toolZone is not null;

    public bool HasSearchZoneForTesting => _searchZone is not null;

    public bool HasBottomDockZoneForTesting => _bottomDockZone is not null;

    public int AppSlotCountForTesting => _appGrid.Controls.Count;

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

    private static IReadOnlyList<LauncherAppEntry> EnsureMinimumSlots(
        IReadOnlyList<LauncherAppEntry> apps,
        int minimumSlots)
    {
        var result = new List<LauncherAppEntry>(apps);
        var index = 1;
        while (result.Count < minimumSlots)
        {
            result.Add(new LauncherAppEntry($"App Slot {index}", string.Empty));
            index++;
        }

        return result;
    }

    private Panel BuildHeroZone(NeoThemeTokens tokens)
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
            Text = "Cloud Sync  |  00:00",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9f, FontStyle.Regular)
        });

        panel.Controls.Add(new Label
        {
            Dock = DockStyle.Top,
            Height = 28,
            Text = "Vstart Next",
            ForeColor = Color.White,
            Font = new Font("Segoe UI Semibold", 14f, FontStyle.Italic)
        });

        return panel;
    }

    private Panel BuildTopTabs(NeoThemeTokens tokens)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = ParseColor(tokens.AccentColor, Color.FromArgb(51, 170, 72))
        };

        var homeButton = CreateTabButton("Home", Color.FromArgb(66, 182, 84));
        homeButton.Dock = DockStyle.Left;
        panel.Controls.Add(homeButton);

        var settingsButton = CreateTabButton("AI", Color.FromArgb(54, 168, 72));
        settingsButton.Dock = DockStyle.Left;
        settingsButton.Click += (_, _) => AiSettingsRequested?.Invoke(this, EventArgs.Empty);
        panel.Controls.Add(settingsButton);
        return panel;
    }

    private static Button CreateTabButton(string text, Color backColor)
    {
        var button = new Button
        {
            Width = 64,
            Text = text,
            FlatStyle = FlatStyle.Flat,
            BackColor = backColor,
            ForeColor = Color.White
        };
        button.FlatAppearance.BorderSize = 0;
        return button;
    }

    private static Panel BuildCategoryZone(NeoThemeTokens tokens)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(236, 236, 236),
            Padding = new Padding(10, 8, 10, 8)
        };

        var rows = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = AppCategories.Count
        };

        foreach (var category in AppCategories)
        {
            rows.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
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

        panel.Controls.Add(rows);
        return panel;
    }

    private FlowLayoutPanel BuildAppGrid(NeoThemeTokens tokens, IReadOnlyList<LauncherAppEntry> apps)
    {
        var grid = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = true,
            AutoScroll = true,
            Padding = new Padding(12, 8, 12, 8)
        };

        foreach (var app in apps)
        {
            var tile = new Panel
            {
                Width = 70,
                Height = 82,
                Margin = new Padding(0, 0, 8, 8),
                BackColor = Color.Transparent
            };
            var title = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 24,
                Text = app.Name,
                TextAlign = ContentAlignment.TopCenter,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular)
            };
            var icon = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ParseColor(tokens.CommandBarColor, Color.FromArgb(164, 198, 235)),
                Margin = new Padding(10),
                BorderStyle = BorderStyle.FixedSingle
            };

            if (!string.IsNullOrWhiteSpace(app.TargetPath))
            {
                void EmitRunCommand(object? _, EventArgs __) =>
                    CommandSubmitted?.Invoke(this, $"run: {app.TargetPath}");
                tile.Click += EmitRunCommand;
                icon.Click += EmitRunCommand;
                title.Click += EmitRunCommand;
            }

            tile.Controls.Add(title);
            tile.Controls.Add(icon);
            grid.Controls.Add(tile);
        }

        return grid;
    }

    private static Panel BuildToolZone(NeoThemeTokens tokens)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(236, 236, 236),
            Padding = new Padding(10, 0, 10, 8)
        };

        var list = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = ToolGroups.Count
        };

        foreach (var group in ToolGroups)
        {
            list.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / ToolGroups.Count));
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

        panel.Controls.Add(list);
        return panel;
    }

    private Panel BuildSearchZone(NeoThemeTokens tokens, out TextBox searchInput)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(10, 6, 10, 6),
            BackColor = Color.FromArgb(230, 230, 230)
        };

        searchInput = new TextBox
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle,
            PlaceholderText = "Search app or command"
        };
        searchInput.KeyDown += SearchInputOnKeyDown;
        var input = searchInput;

        var searchButton = new Button
        {
            Dock = DockStyle.Right,
            Width = 40,
            Text = "Q",
            FlatStyle = FlatStyle.Flat,
            BackColor = ParseColor(tokens.AccentColor, Color.FromArgb(51, 170, 72)),
            ForeColor = Color.White
        };
        searchButton.FlatAppearance.BorderSize = 0;
        searchButton.Click += (_, _) => CommandSubmitted?.Invoke(this, input.Text);

        panel.Controls.Add(searchInput);
        panel.Controls.Add(searchButton);
        return panel;
    }

    private static Panel BuildBottomDockZone(NeoThemeTokens tokens)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = ParseColor(tokens.AccentColor, Color.FromArgb(51, 170, 72)),
            Padding = new Padding(6, 8, 6, 8)
        };

        var bar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = false,
            FlowDirection = FlowDirection.LeftToRight
        };

        foreach (var icon in new[] { "V", "[]", "O", "^", "+", "*", "o", "C" })
        {
            bar.Controls.Add(new Label
            {
                Width = 30,
                Height = 24,
                Text = icon,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 11f, FontStyle.Regular)
            });
        }

        panel.Controls.Add(bar);
        return panel;
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
