using VStartNext.Core.Agent;

namespace VStartNext.App.Windows;

public sealed class AgentExecutionProgressForm : Form
{
    private readonly Func<CancellationToken, IProgress<AgentExecutionUpdate>, Task<AgentRunResult>> _runner;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly ListView _progressView = new();
    private readonly Button _cancelButton = new();

    public AgentExecutionProgressForm(
        AgentExecutionPreview preview,
        Func<CancellationToken, IProgress<AgentExecutionUpdate>, Task<AgentRunResult>> runner)
    {
        _runner = runner;

        Text = "Agent Execution Progress";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Width = 780;
        Height = 470;

        var header = new Label
        {
            Dock = DockStyle.Top,
            Height = 44,
            Padding = new Padding(12, 12, 12, 8),
            Text = $"Running: {preview.Input}"
        };

        _progressView.Dock = DockStyle.Fill;
        _progressView.View = View.Details;
        _progressView.FullRowSelect = true;
        _progressView.Columns.Add("Step", 70);
        _progressView.Columns.Add("Tool", 160);
        _progressView.Columns.Add("Arguments", 300);
        _progressView.Columns.Add("State", 110);
        _progressView.Columns.Add("Message", 120);

        var footer = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 58,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(8),
            WrapContents = false
        };

        _cancelButton.Text = "Cancel";
        _cancelButton.AutoSize = true;
        _cancelButton.Click += (_, _) => TriggerCancelForTesting();
        footer.Controls.Add(_cancelButton);

        Controls.Add(_progressView);
        Controls.Add(footer);
        Controls.Add(header);

        Shown += (_, _) => _ = RunExecutionAsync();
        FormClosed += (_, _) => _cancellationTokenSource.Cancel();
    }

    public bool CancelRequested { get; private set; }
    public AgentRunResult? RunResult { get; private set; }

    public void TriggerCancelForTesting()
    {
        CancelRequested = true;
        _cancelButton.Enabled = false;
        _cancellationTokenSource.Cancel();
    }

    private async Task RunExecutionAsync()
    {
        try
        {
            var progress = new Progress<AgentExecutionUpdate>(AppendProgress);
            RunResult = await _runner(_cancellationTokenSource.Token, progress);
        }
        catch (OperationCanceledException)
        {
            RunResult = new AgentRunResult(false, "Execution canceled", []);
        }
        catch (Exception ex)
        {
            RunResult = new AgentRunResult(false, $"Execution failed: {ex.Message}", []);
        }
        finally
        {
            if (!IsDisposed)
            {
                DialogResult = RunResult?.Success == true ? DialogResult.OK : DialogResult.Cancel;
                Close();
            }
        }
    }

    private void AppendProgress(AgentExecutionUpdate update)
    {
        var item = new ListViewItem($"{update.StepIndex}/{update.TotalSteps}");
        item.SubItems.Add(update.ToolName);
        item.SubItems.Add(update.Arguments);
        item.SubItems.Add(update.State);
        item.SubItems.Add(update.Message);
        _progressView.Items.Add(item);
        _progressView.EnsureVisible(_progressView.Items.Count - 1);
    }
}
