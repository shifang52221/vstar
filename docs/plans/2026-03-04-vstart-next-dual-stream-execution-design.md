# VStart Next Dual-Stream Execution Design

Date: 2026-03-04
Status: Approved
Priority: High (AI-first interaction quality)

## 1. Objective

Deliver a two-lane execution experience that feels modern and AI-native:

1. Model stream (high-level reasoning/status text, real-time)
2. Tool stream (step-level execution trace, real-time)

Keep existing one-click cancel behavior and preserve current app routing.

## 2. Scope

In scope:

1. Upgrade execution progress dialog to dual-stream layout.
2. Render model stream as timeline text updates.
3. Keep tool stream as structured list of step updates.
4. Ensure cancel state is reflected in both streams.
5. Add targeted tests for dual-stream rendering and cancel behavior.

Out of scope:

1. Provider-level token streaming (SSE/chunk text from cloud model).
2. Planner protocol redesign.
3. New background service/process model.

## 3. Architecture

### 3.1 Data Contract

Reuse existing `AgentExecutionUpdate` for tool-step progress events. Model stream v1 is generated from execution lifecycle and tool events in the progress form.

### 3.2 UI Composition

`AgentExecutionProgressForm` uses a split layout:

1. Top lane: read-only model stream text area.
2. Bottom lane: existing step table (tool, args, state, message).

### 3.3 Flow

1. Gateway opens preview and execution mode.
2. Progress dialog starts run callback.
3. During run:
   - Tool events append to tool stream.
   - Model stream receives lifecycle and interpreted status lines.
4. On cancel/failure/success, final model line is appended.
5. Dialog returns final `AgentRunResult`.

## 4. Error Handling

1. `OperationCanceledException` maps to `Execution canceled` result and model line.
2. Unexpected exceptions map to `Execution failed: <message>` and model line.
3. Form-close while running triggers cancellation token.

## 5. Testing Strategy

1. Keep existing gateway/executor tests green.
2. Add progress-form tests covering:
   - tool updates append to tool lane
   - model lane appends lifecycle lines
   - cancel action sets state and appends cancel line

## 6. Rollout Notes

This design intentionally ships model status streaming first, then adds token-level model streaming in a follow-up task without breaking the dialog contract.

