using System.Globalization;
using System.Windows.Forms;
using VStartNext.App.Settings;

namespace VStartNext.App.Windows;

public sealed class ModelSettingsForm : Form
{
    private readonly IModelSettingsService _service;
    private readonly ComboBox _providerCombo = new();
    private readonly TextBox _baseUrlText = new();
    private readonly TextBox _apiKeyText = new();
    private readonly TextBox _chatModelText = new();
    private readonly TextBox _plannerModelText = new();
    private readonly TextBox _reflectionModelText = new();
    private readonly TextBox _temperatureText = new();
    private readonly TextBox _maxTokensText = new();
    private readonly TextBox _timeoutText = new();
    private readonly TextBox _retryText = new();

    public ModelSettingsForm(IModelSettingsService service)
    {
        _service = service;

        Text = "AI Model Settings";
        StartPosition = FormStartPosition.CenterParent;
        Width = 620;
        Height = 560;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 12,
            Padding = new Padding(12)
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (var i = 0; i < 11; i++)
        {
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        }
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _providerCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        _providerCombo.Items.Add("OpenAiCompatible");

        _apiKeyText.UseSystemPasswordChar = true;

        AddRow(root, 0, "Provider", _providerCombo);
        AddRow(root, 1, "Base URL", _baseUrlText);
        AddRow(root, 2, "API Key", _apiKeyText);
        AddRow(root, 3, "Chat Model", _chatModelText);
        AddRow(root, 4, "Planner Model", _plannerModelText);
        AddRow(root, 5, "Reflection Model", _reflectionModelText);
        AddRow(root, 6, "Temperature", _temperatureText);
        AddRow(root, 7, "Max Tokens", _maxTokensText);
        AddRow(root, 8, "Timeout Seconds", _timeoutText);
        AddRow(root, 9, "Retry Count", _retryText);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Padding = new Padding(0, 6, 0, 0)
        };

        var cancelButton = new Button { Text = "Cancel", Width = 96 };
        cancelButton.Click += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };

        var saveButton = new Button { Text = "Save", Width = 96 };
        saveButton.Click += SaveButtonOnClick;

        var testButton = new Button { Text = "Test Connection", Width = 132 };
        testButton.Click += TestButtonOnClick;

        actions.Controls.Add(cancelButton);
        actions.Controls.Add(saveButton);
        actions.Controls.Add(testButton);

        root.Controls.Add(actions, 0, 10);
        root.SetColumnSpan(actions, 2);

        Controls.Add(root);
        LoadValues();
    }

    private void LoadValues()
    {
        var data = _service.Load();
        _providerCombo.SelectedItem = data.Provider;
        if (_providerCombo.SelectedItem is null && _providerCombo.Items.Count > 0)
        {
            _providerCombo.SelectedIndex = 0;
        }

        _baseUrlText.Text = data.BaseUrl;
        _apiKeyText.Text = data.ApiKey;
        _chatModelText.Text = data.ChatModel;
        _plannerModelText.Text = data.PlannerModel;
        _reflectionModelText.Text = data.ReflectionModel;
        _temperatureText.Text = data.Temperature.ToString(CultureInfo.InvariantCulture);
        _maxTokensText.Text = data.MaxTokens.ToString(CultureInfo.InvariantCulture);
        _timeoutText.Text = data.TimeoutSeconds.ToString(CultureInfo.InvariantCulture);
        _retryText.Text = data.RetryCount.ToString(CultureInfo.InvariantCulture);
    }

    private async void TestButtonOnClick(object? sender, EventArgs e)
    {
        var input = BuildInput();
        var result = await _service.TestConnectionAsync(input);
        var text = result.Success
            ? "Connection succeeded."
            : $"Connection failed: {result.Message}";
        var icon = result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning;

        MessageBox.Show(this, text, "Model Connection", MessageBoxButtons.OK, icon);
    }

    private void SaveButtonOnClick(object? sender, EventArgs e)
    {
        var input = BuildInput();
        _service.Save(input);
        MessageBox.Show(this, "Model settings saved.", "AI Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
        DialogResult = DialogResult.OK;
        Close();
    }

    private ModelSettingsInput BuildInput()
    {
        return new ModelSettingsInput
        {
            Provider = _providerCombo.SelectedItem?.ToString() ?? "OpenAiCompatible",
            BaseUrl = _baseUrlText.Text.Trim(),
            ApiKey = _apiKeyText.Text.Trim(),
            ChatModel = _chatModelText.Text.Trim(),
            PlannerModel = _plannerModelText.Text.Trim(),
            ReflectionModel = _reflectionModelText.Text.Trim(),
            Temperature = ParseDouble(_temperatureText.Text, 0.2),
            MaxTokens = ParseInt(_maxTokensText.Text, 2048),
            TimeoutSeconds = ParseInt(_timeoutText.Text, 30),
            RetryCount = ParseInt(_retryText.Text, 2)
        };
    }

    private static double ParseDouble(string input, double fallback)
    {
        return double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
            ? value
            : fallback;
    }

    private static int ParseInt(string input, int fallback)
    {
        return int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : fallback;
    }

    private static void AddRow(TableLayoutPanel layout, int row, string labelText, Control input)
    {
        var label = new Label
        {
            Text = labelText,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };

        input.Dock = DockStyle.Fill;
        layout.Controls.Add(label, 0, row);
        layout.Controls.Add(input, 1, row);
    }
}
