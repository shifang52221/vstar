using System.Drawing;
using System.Windows.Forms;
using VStartNext.App.Styles;

namespace VStartNext.App.Windows.Controls;

public sealed class CategoryRailControl : UserControl
{
    private static readonly IReadOnlyList<string> Categories =
    [
        "Home",
        "Apps",
        "Folders",
        "Flows",
        "Agents"
    ];

    public CategoryRailControl(NeoThemeTokens? tokens = null)
    {
        var theme = tokens ?? NeoThemeTokens.Default();

        Dock = DockStyle.Fill;
        Margin = new Padding(6);
        Padding = new Padding(8, 12, 8, 8);
        BackColor = ParseColor(theme.PanelColor, Color.FromArgb(34, 36, 44));

        var stack = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = Categories.Count + 1
        };
        stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));

        stack.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            Text = "Library",
            Font = new Font("Segoe UI Semibold", 10f, FontStyle.Regular),
            ForeColor = ParseColor(theme.TextPrimaryColor, Color.FromArgb(228, 234, 242)),
            TextAlign = ContentAlignment.MiddleLeft
        });

        foreach (var category in Categories)
        {
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            var button = new Button
            {
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                Text = category,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = ParseColor(theme.CommandInputColor, Color.FromArgb(26, 29, 36)),
                ForeColor = ParseColor(theme.TextSecondaryColor, Color.FromArgb(196, 210, 224))
            };
            button.FlatAppearance.BorderColor = ParseColor(theme.PanelColor, Color.FromArgb(62, 72, 86));
            stack.Controls.Add(button);
        }

        Controls.Add(stack);
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
