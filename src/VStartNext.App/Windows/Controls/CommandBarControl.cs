using System.Drawing;
using System.Windows.Forms;
using VStartNext.App.Styles;

namespace VStartNext.App.Windows.Controls;

public sealed class CommandBarControl : UserControl
{
    public TextBox InputBox { get; }
    public event EventHandler<string>? CommandSubmitted;

    public CommandBarControl(NeoThemeTokens? tokens = null)
    {
        var theme = tokens ?? NeoThemeTokens.Default();
        Dock = DockStyle.Fill;
        Margin = new Padding(6);
        BackColor = ParseColor(theme.CommandBarColor, Color.FromArgb(28, 30, 38));

        InputBox = new TextBox
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 11, FontStyle.Regular),
            ForeColor = ParseColor(theme.TextPrimaryColor, Color.White),
            BackColor = ParseColor(theme.CommandInputColor, Color.FromArgb(22, 23, 30)),
            PlaceholderText = "Search apps, files, commands, or ask AI..."
        };

        var padding = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
        padding.Controls.Add(InputBox);
        Controls.Add(padding);

        InputBox.KeyDown += InputBoxOnKeyDown;
    }

    private void InputBoxOnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
        {
            return;
        }

        e.SuppressKeyPress = true;
        e.Handled = true;
        CommandSubmitted?.Invoke(this, InputBox.Text);
    }

    public void SubmitForTesting(string input)
    {
        InputBox.Text = input;
        CommandSubmitted?.Invoke(this, InputBox.Text);
    }

    public void FocusInput()
    {
        InputBox.Focus();
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
