using System.Drawing;
using System.Windows.Forms;
using VStartNext.App.Styles;

namespace VStartNext.App.Windows;

public sealed class WinUiPreviewShellForm : Form, IShellWindow
{
    private readonly Panel _header;
    private readonly NeoThemeTokens _tokens;
    private readonly NeoPanelView _neoPanel;
    private readonly Label _aiBadgeLabel;
    private readonly Label _modelProfileLabel;
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

        var headerMeta = new Panel
        {
            Dock = DockStyle.Right,
            Width = 360,
            Padding = new Padding(0),
            BackColor = Color.Transparent
        };
        _modelProfileLabel = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            ForeColor = ParseColor(_tokens.TextSecondaryColor, Color.FromArgb(174, 188, 208)),
            Font = new Font("Segoe UI", 9f, FontStyle.Regular),
            Text = "OpenAiCompatible / gpt-4.1-mini"
        };
        _aiBadgeLabel = new Label
        {
            Dock = DockStyle.Right,
            Width = 92,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = ParseColor(_tokens.TextPrimaryColor, Color.FromArgb(230, 240, 248)),
            BackColor = ParseColor(_tokens.CommandBarColor, Color.FromArgb(28, 30, 38)),
            Font = new Font("Segoe UI Semibold", 8.5f, FontStyle.Regular),
            Text = "Cloud AI"
        };
        headerMeta.Controls.Add(_modelProfileLabel);
        headerMeta.Controls.Add(_aiBadgeLabel);
        _header.Controls.Add(headerMeta);

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

    public string AiBadgeTextForTesting => _aiBadgeLabel.Text;

    public string ModelProfileTextForTesting => _modelProfileLabel.Text;

    public void TriggerHeaderInteractionForTesting()
    {
        HandleHeaderInteraction();
    }

    public void SetModelProfile(string provider, string chatModel)
    {
        var safeProvider = string.IsNullOrWhiteSpace(provider) ? "OpenAiCompatible" : provider.Trim();
        var safeModel = string.IsNullOrWhiteSpace(chatModel) ? "gpt-4.1-mini" : chatModel.Trim();
        _modelProfileLabel.Text = $"{safeProvider} / {safeModel}";
    }

    public void SetModelProfileForTesting(string provider, string chatModel)
    {
        SetModelProfile(provider, chatModel);
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
