using System.Drawing;
using System.Windows.Forms;

namespace VStartNext.App.Windows.Controls;

public sealed class ContextPanelControl : UserControl
{
    private readonly Button _aiSettingsButton;

    public event EventHandler? AiSettingsRequested;

    public ContextPanelControl()
    {
        Dock = DockStyle.Fill;
        Margin = new Padding(6);
        BackColor = Color.FromArgb(32, 35, 42);

        _aiSettingsButton = new Button
        {
            Dock = DockStyle.Top,
            Height = 32,
            Text = "AI Settings",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(44, 50, 61),
            ForeColor = Color.FromArgb(228, 233, 240),
            Margin = new Padding(8),
            Padding = new Padding(8, 0, 8, 0)
        };
        _aiSettingsButton.FlatAppearance.BorderColor = Color.FromArgb(70, 84, 102);
        _aiSettingsButton.Click += (_, _) => AiSettingsRequested?.Invoke(this, EventArgs.Empty);

        Controls.Add(_aiSettingsButton);
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

    public void TriggerAiSettingsForTesting()
    {
        _aiSettingsButton.PerformClick();
    }
}
