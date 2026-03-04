# Agent Execution Status Stream Design

## Goal

Add a resilient, adjustable status layer to the execution progress dialog so users can see live AI phase and cumulative stream-token count without coupling UI rendering to execution control logic.

## Why Now

The current progress dialog already shows planning/finalizing token text and tool-step updates, but it does not provide a single explicit execution phase indicator or any quantitative stream signal. This makes it harder to understand "where the AI is now" during longer runs.

## Scope

- Add execution phase state machine for the progress dialog:
  - `Planning`
  - `Running`
  - `Finalizing`
  - `Completed`
  - `Canceled`
  - `Failed`
- Add cumulative token counter driven by streamed text chunks.
- Render both phase and token count in a dedicated status strip at the top of `AgentExecutionProgressForm`.
- Keep existing transcript and tool-step list behavior unchanged.

## Non-Goals

- No precise tokenizer integration in this wave.
- No token/s metric in this wave.
- No protocol or model-router API changes.
- No visual overhaul of other forms.

## Architecture

1. Introduce a small `ExecutionStatusViewModel` in the WinForms app layer:
   - holds current phase
   - accumulates stream token count
   - produces display text for UI binding/adaptation
2. `AgentExecutionProgressForm` updates the view model from existing flow events:
   - before planning stream -> `Planning`
   - first tool running update -> `Running`
   - before finalizing stream -> `Finalizing`
   - final success/failure/cancel -> terminal phase
   - each streamed chunk -> increment token counter
3. Form reads view-model display values and updates labels.

This keeps future adjustments local: adding new phases/metrics requires view-model changes and minimal form wiring.

## Error Handling

- Empty token chunks are ignored.
- Stream exceptions keep existing fallback behavior and do not crash dialog.
- Cancellation transitions to `Canceled` and preserves existing run result semantics.

## Testing Strategy

- Extend `AgentExecutionProgressFormTests`:
  - verifies final phase = `Completed` and cumulative token count after planning/finalizing streams
  - verifies cancellation transitions phase to `Canceled`
  - verifies exception/failure transitions to `Failed`
- Add unit tests for the new status view model:
  - phase transitions
  - token accumulation
  - generated display text

## Rollout

- Keep this behind existing execution dialog flow (no flag needed).
- Low risk: UI-only plus test coverage.
