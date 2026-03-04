using VStartNext.Core.Agent;

namespace VStartNext.App.Windows;

public sealed class AgentExecutionPreviewForm : Form
{
    private readonly ListView _stepsView = new();
    private readonly Button _executeAllButton = new();
    private readonly Button _singleStepButton = new();
    private readonly Button _cancelButton = new();

    public AgentExecutionPreviewForm(AgentExecutionPreview preview)
    {
        SelectedMode = AgentExecutionMode.Cancel;

        Text = "Agent Execution Preview";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Width = 760;
        Height = 460;

        var header = new Label
        {
            Dock = DockStyle.Top,
            Height = 44,
            Padding = new Padding(12, 12, 12, 8),
            Text = "Review planned actions before execution."
        };

        _stepsView.Dock = DockStyle.Fill;
        _stepsView.View = View.Details;
        _stepsView.FullRowSelect = true;
        _stepsView.Columns.Add("Index", 60);
        _stepsView.Columns.Add("Tool", 180);
        _stepsView.Columns.Add("Arguments", 340);
        _stepsView.Columns.Add("Risk", 120);
        foreach (var (step, index) in preview.Steps.Select((step, index) => (step, index)))
        {
            var item = new ListViewItem((index + 1).ToString());
            item.SubItems.Add(step.ToolName);
            item.SubItems.Add(step.Arguments);
            item.SubItems.Add(step.RiskLevel.ToString());
            _stepsView.Items.Add(item);
        }

        var actionsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 58,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(8),
            WrapContents = false
        };

        _executeAllButton.Text = "Execute All";
        _executeAllButton.AutoSize = true;
        _executeAllButton.Click += (_, _) => SelectModeAndClose(AgentExecutionMode.ExecuteAll);

        _singleStepButton.Text = "Single Step";
        _singleStepButton.AutoSize = true;
        _singleStepButton.Click += (_, _) => SelectModeAndClose(AgentExecutionMode.ExecuteSingleStep);

        _cancelButton.Text = "Cancel";
        _cancelButton.AutoSize = true;
        _cancelButton.Click += (_, _) => SelectModeAndClose(AgentExecutionMode.Cancel);

        actionsPanel.Controls.Add(_executeAllButton);
        actionsPanel.Controls.Add(_singleStepButton);
        actionsPanel.Controls.Add(_cancelButton);

        Controls.Add(_stepsView);
        Controls.Add(actionsPanel);
        Controls.Add(header);
    }

    public AgentExecutionMode SelectedMode { get; private set; }

    public void TriggerExecuteAllForTesting() => SelectModeForTesting(AgentExecutionMode.ExecuteAll);

    public void TriggerSingleStepForTesting() => SelectModeForTesting(AgentExecutionMode.ExecuteSingleStep);

    public void TriggerCancelForTesting() => SelectModeForTesting(AgentExecutionMode.Cancel);

    private void SelectModeForTesting(AgentExecutionMode mode)
    {
        SelectedMode = mode;
    }

    private void SelectModeAndClose(AgentExecutionMode mode)
    {
        SelectedMode = mode;
        DialogResult = mode == AgentExecutionMode.Cancel ? DialogResult.Cancel : DialogResult.OK;
        Close();
    }
}
