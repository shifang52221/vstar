using VStartNext.Core.Agent;
using System.Text;
using VStartNext.App.Windows.Execution;

namespace VStartNext.App.Windows;

public sealed class AgentExecutionProgressForm : Form
{
    private readonly Func<CancellationToken, IProgress<AgentExecutionUpdate>, Task<AgentRunResult>> _runner;
    private readonly Func<CancellationToken, IAsyncEnumerable<string>>? _planningTokenStream;
    private readonly Func<AgentRunResult, CancellationToken, IAsyncEnumerable<string>>? _finalizingTokenStream;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly List<string> _modelLines = [];
    private readonly List<AgentExecutionUpdate> _toolUpdates = [];
    private readonly StringBuilder _modelTranscript = new();
    private readonly ExecutionStatusViewModel _statusViewModel = new();
    private readonly TextBox _modelStreamBox = new();
    private readonly ListView _progressView = new();
    private readonly Button _cancelButton = new();
    private readonly Label _statusPhaseValue = new();
    private readonly Label _statusTokenValue = new();

    public AgentExecutionProgressForm(
        AgentExecutionPreview preview,
        Func<CancellationToken, IProgress<AgentExecutionUpdate>, Task<AgentRunResult>> runner,
        Func<CancellationToken, IAsyncEnumerable<string>>? planningTokenStream = null,
        Func<AgentRunResult, CancellationToken, IAsyncEnumerable<string>>? finalizingTokenStream = null)
    {
        _runner = runner;
        _planningTokenStream = planningTokenStream;
        _finalizingTokenStream = finalizingTokenStream;

        Text = "Agent Execution Progress";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Width = 780;
        Height = 560;

        var header = new Label
        {
            Dock = DockStyle.Top,
            Height = 44,
            Padding = new Padding(12, 12, 12, 8),
            Text = $"Running: {preview.Input}"
        };

        var statusStrip = new Panel
        {
            Dock = DockStyle.Top,
            Height = 34,
            Padding = new Padding(12, 4, 12, 4),
            BackColor = SystemColors.ControlLight
        };
        var statusPhaseTitle = new Label
        {
            AutoSize = true,
            Dock = DockStyle.Left,
            Text = "Phase:",
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(0, 6, 6, 0)
        };
        _statusPhaseValue.AutoSize = true;
        _statusPhaseValue.Dock = DockStyle.Left;
        _statusPhaseValue.Font = new Font("Segoe UI Semibold", 9f, FontStyle.Regular);
        _statusPhaseValue.Padding = new Padding(0, 6, 0, 0);

        _statusTokenValue.AutoSize = true;
        _statusTokenValue.Dock = DockStyle.Right;
        _statusTokenValue.TextAlign = ContentAlignment.MiddleRight;
        _statusTokenValue.Padding = new Padding(0, 6, 0, 0);

        statusStrip.Controls.Add(_statusTokenValue);
        statusStrip.Controls.Add(_statusPhaseValue);
        statusStrip.Controls.Add(statusPhaseTitle);

        _modelStreamBox.Dock = DockStyle.Fill;
        _modelStreamBox.ReadOnly = true;
        _modelStreamBox.Multiline = true;
        _modelStreamBox.ScrollBars = ScrollBars.Vertical;
        _modelStreamBox.BackColor = SystemColors.Window;

        _progressView.Dock = DockStyle.Fill;
        _progressView.View = View.Details;
        _progressView.FullRowSelect = true;
        _progressView.Columns.Add("Step", 70);
        _progressView.Columns.Add("Tool", 160);
        _progressView.Columns.Add("Arguments", 300);
        _progressView.Columns.Add("State", 110);
        _progressView.Columns.Add("Message", 120);

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 210
        };
        split.Panel1.Padding = new Padding(8, 8, 8, 4);
        split.Panel2.Padding = new Padding(8, 4, 8, 8);
        split.Panel1.Controls.Add(_modelStreamBox);
        split.Panel2.Controls.Add(_progressView);

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

        Controls.Add(split);
        Controls.Add(footer);
        Controls.Add(statusStrip);
        Controls.Add(header);
        ApplyStatusToUi();

        Shown += (_, _) => _ = RunExecutionAsync();
        FormClosed += (_, _) => _cancellationTokenSource.Cancel();
    }

    public bool CancelRequested { get; private set; }
    public AgentRunResult? RunResult { get; private set; }
    public int ToolUpdateCountForTesting => _toolUpdates.Count;
    public IReadOnlyList<string> ModelLinesForTesting => _modelLines;
    public string ModelStreamTextForTesting => _modelTranscript.ToString();
    public string StatusPhaseTextForTesting => _statusViewModel.PhaseText;
    public int StreamTokenCountForTesting => _statusViewModel.StreamTokenCount;

    public void TriggerCancelForTesting()
    {
        CancelRequested = true;
        SetStatusPhase(ExecutionStatusPhase.Canceled);
        _cancelButton.Enabled = false;
        AppendModelLine("Cancel requested by user.");
        _cancellationTokenSource.Cancel();
    }

    public Task RunForTestingAsync()
    {
        return RunExecutionAsync();
    }

    private async Task RunExecutionAsync()
    {
        SetStatusPhase(ExecutionStatusPhase.Planning);
        AppendModelLine("Agent execution started.");
        await AppendTokenStreamAsync(
            "Planning",
            ExecutionStatusPhase.Planning,
            _planningTokenStream,
            "Planning stream unavailable.");
        try
        {
            var progress = new UiProgress(this);
            RunResult = await _runner(_cancellationTokenSource.Token, progress);
            if (RunResult is null)
            {
                RunResult = new AgentRunResult(false, "Execution failed", []);
            }

            var runResult = RunResult!;
            if (!_cancellationTokenSource.IsCancellationRequested &&
                runResult is not null &&
                _finalizingTokenStream is not null)
            {
                await AppendTokenStreamAsync(
                    "Finalizing",
                    ExecutionStatusPhase.Finalizing,
                    cancellationToken => _finalizingTokenStream(runResult, cancellationToken),
                    "Finalizing stream unavailable.");
            }

            if (RunResult is { } finalResult)
            {
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    SetStatusPhase(ExecutionStatusPhase.Canceled);
                }
                else
                {
                    SetStatusPhase(finalResult.Success
                        ? ExecutionStatusPhase.Completed
                        : ExecutionStatusPhase.Failed);
                }

                AppendModelLine(finalResult.Success
                    ? "Agent execution completed."
                    : $"Agent execution failed: {finalResult.Message}");
            }
        }
        catch (OperationCanceledException)
        {
            SetStatusPhase(ExecutionStatusPhase.Canceled);
            RunResult = new AgentRunResult(false, "Execution canceled", []);
            AppendModelLine("Agent execution canceled.");
        }
        catch (Exception ex)
        {
            SetStatusPhase(ExecutionStatusPhase.Failed);
            RunResult = new AgentRunResult(false, $"Execution failed: {ex.Message}", []);
            AppendModelLine($"Agent execution failed: {ex.Message}");
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

    private async Task AppendTokenStreamAsync(
        string phase,
        ExecutionStatusPhase phaseStatus,
        Func<CancellationToken, IAsyncEnumerable<string>>? streamFactory,
        string fallbackMessage)
    {
        if (streamFactory is null)
        {
            return;
        }

        SetStatusPhase(phaseStatus);
        AppendModelLine($"{phase}...");
        try
        {
            var hasToken = false;
            await foreach (var token in streamFactory(_cancellationTokenSource.Token)
                               .WithCancellation(_cancellationTokenSource.Token))
            {
                if (string.IsNullOrEmpty(token))
                {
                    continue;
                }

                hasToken = true;
                AppendModelToken(token);
            }

            if (hasToken)
            {
                _modelStreamBox.AppendText(Environment.NewLine);
            }
            else
            {
                AppendModelLine(fallbackMessage);
            }
        }
        catch (OperationCanceledException) when (_cancellationTokenSource.IsCancellationRequested)
        {
            // no-op; cancellation line is written by the main flow
        }
        catch
        {
            AppendModelLine(fallbackMessage);
        }
    }

    private void AppendProgress(AgentExecutionUpdate update)
    {
        _toolUpdates.Add(update);

        var item = new ListViewItem($"{update.StepIndex}/{update.TotalSteps}");
        item.SubItems.Add(update.ToolName);
        item.SubItems.Add(update.Arguments);
        item.SubItems.Add(update.State);
        item.SubItems.Add(update.Message);
        _progressView.Items.Add(item);
        _progressView.EnsureVisible(_progressView.Items.Count - 1);

        switch (update.State)
        {
            case "Running":
                SetStatusPhase(ExecutionStatusPhase.Running);
                AppendModelLine(
                    $"Running step {update.StepIndex}/{update.TotalSteps}: {update.ToolName}({update.Arguments})");
                break;
            case "Succeeded":
                AppendModelLine(
                    $"Step {update.StepIndex}/{update.TotalSteps} succeeded: {update.ToolName}.");
                break;
            case "Failed":
                AppendModelLine(
                    $"Step {update.StepIndex}/{update.TotalSteps} failed: {update.ToolName}. {update.Message}");
                break;
        }
    }

    private void ReportProgress(AgentExecutionUpdate update)
    {
        if (IsDisposed)
        {
            return;
        }

        if (InvokeRequired)
        {
            Invoke(new Action<AgentExecutionUpdate>(ReportProgress), update);
            return;
        }

        AppendProgress(update);
    }

    private void AppendModelLine(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        _modelLines.Add(message);
        _modelTranscript.AppendLine(message);
        _modelStreamBox.AppendText(message + Environment.NewLine);
    }

    private void AppendModelToken(string token)
    {
        _statusViewModel.AddStreamToken(token);
        ApplyStatusToUi();
        _modelTranscript.Append(token);
        _modelStreamBox.AppendText(token);
    }

    private void SetStatusPhase(ExecutionStatusPhase phase)
    {
        _statusViewModel.SetPhase(phase);
        ApplyStatusToUi();
    }

    private void ApplyStatusToUi()
    {
        _statusPhaseValue.Text = _statusViewModel.PhaseText;
        _statusTokenValue.Text = _statusViewModel.StreamTokenText;
    }

    private sealed class UiProgress : IProgress<AgentExecutionUpdate>
    {
        private readonly AgentExecutionProgressForm _form;

        public UiProgress(AgentExecutionProgressForm form)
        {
            _form = form;
        }

        public void Report(AgentExecutionUpdate value)
        {
            _form.ReportProgress(value);
        }
    }
}
