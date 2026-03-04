using System.Drawing;
using System.Windows.Forms;
using VStartNext.App.Styles;

namespace VStartNext.App.Windows;

public sealed class WinUiPreviewShellForm : Form, IShellWindow
{
    private readonly Panel _header;
    private readonly NeoThemeTokens _tokens;
    private readonly NeoPanelView _neoPanel;
    private Action? _onOpenModelSettings;
    private int _headerInteractionCountForTesting;

    public event EventHandler<string>? CommandSubmitted;

    public WinUiPreviewShellForm(NeoThemeTokens? tokens = null, Action? onOpenModelSettings = null)
    {
        _tokens = tokens ?? NeoThemeTokens.Default();
        _onOpenModelSettings = onOpenModelSettings;
        Text = "VStart Next - WinUI Preview";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(1080, 680);
        MinimumSize = new Size(1080, 680);
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = false;
        BackColor = ParseColor(_tokens.BackgroundColor, Color.FromArgb(12, 16, 24));

        _header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 44,
            Cursor = Cursors.Hand,
            BackColor = ParseColor(_tokens.HeaderColor, Color.FromArgb(18, 26, 38)),
            Padding = new Padding(_tokens.SpacingMd, 10, _tokens.SpacingMd, 8)
        };
        var headerTitle = new Label
        {
            Dock = DockStyle.Left,
            AutoSize = false,
            Width = 340,
            Cursor = Cursors.Hand,
            ForeColor = ParseColor(_tokens.TextPrimaryColor, Color.FromArgb(230, 240, 248)),
            Font = new Font("Segoe UI Semibold", 10f, FontStyle.Regular),
            Text = "WinUI Preview Shell"
        };
        _header.Controls.Add(headerTitle);

        _header.Click += (_, _) => HandleHeaderInteraction();
        headerTitle.Click += (_, _) => HandleHeaderInteraction();

        _neoPanel = new NeoPanelView(_tokens);
        _neoPanel.Dock = DockStyle.Fill;
        _neoPanel.CommandSubmitted += (_, input) => CommandSubmitted?.Invoke(this, input);
        _neoPanel.AiSettingsRequested += (_, _) => _onOpenModelSettings?.Invoke();

        Controls.Add(_neoPanel);
        Controls.Add(_header);
    }

    public void ShowShell()
    {
        if (!Visible)
        {
            Show();
        }

        Activate();
    }

    public void HideShell()
    {
        Hide();
    }

    public void SetOpenModelSettingsHandler(Action onOpenModelSettings)
    {
        _onOpenModelSettings = onOpenModelSettings;
    }

    public void SubmitCommandForTesting(string input)
    {
        _neoPanel.SubmitCommandForTesting(input);
    }

    public void TriggerAiSettingsForTesting()
    {
        _neoPanel.TriggerAiSettingsForTesting();
    }

    public NeoThemeTokens ThemeTokensForTesting => _tokens;

    public Color HeaderBackColorForTesting => _header.BackColor;

    public int HeaderInteractionCountForTesting => _headerInteractionCountForTesting;

    public bool CommandFocusRequestedForTesting => _neoPanel.FocusCommandRequestedForTesting;

    public void TriggerHeaderInteractionForTesting()
    {
        HandleHeaderInteraction();
    }

    private void HandleHeaderInteraction()
    {
        _headerInteractionCountForTesting++;
        _neoPanel.FocusCommandInput();
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
