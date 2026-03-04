# VStart Next Token Streaming Design

Date: 2026-03-04
Status: Approved (owner-decided)
Priority: High

## 1. Goal

Add cloud-model token streaming (SSE) to the execution dialog model lane, covering:

1. Planning phase stream
2. Finalizing phase stream

while preserving tool-step streaming and one-click cancel.

## 2. Constraints

1. Keep existing execution pipeline stable.
2. Do not block tool execution if streaming fails.
3. Keep current preview/confirm flow unchanged.

## 3. Scope

In scope:

1. `IAgentModelRouter` streaming contract.
2. OpenAI-compatible SSE token parsing.
3. Progress dialog token rendering for planning/finalizing phases.
4. Graceful fallback to non-stream behavior.
5. Tests for SSE parsing and dialog token rendering.

Out of scope:

1. Rebuilding planner confirmation flow.
2. Multi-provider stream normalization framework.
3. Background worker process.

## 4. Architecture

### 4.1 Router Contract

Add async token stream API:

1. `CompleteAsync(prompt)` remains for compatibility and fallback.
2. `StreamCompletionAsync(prompt, cancellationToken)` returns token sequence.

### 4.2 Router Implementation

`OpenAiCompatibleAgentModelRouter` sends OpenAI-compatible streaming request (`stream=true`) and parses SSE lines:

1. Read `data:` chunks
2. Stop on `[DONE]`
3. Extract `choices[0].delta.content`
4. Yield non-empty token strings

### 4.3 UI Integration

`AgentExecutionProgressForm` receives optional delegates:

1. planning token stream factory
2. finalizing token stream factory

Flow:

1. Render planning phase title and tokens
2. Execute tools with existing step progress
3. Render finalizing phase title and tokens
4. End with success/failure/cancel line

### 4.4 Prompt Strategy

Since preview is confirmed before execution dialog opens, planning stream is generated as a tokenized plan narrative from the preview plan. Finalizing stream is generated from run result.

## 5. Error Handling and Fallback

1. If streaming throws (network/provider/parse), append fallback status line and continue.
2. If canceled, stop stream immediately and append canceled line.
3. Tool execution result remains source of truth for success/failure.

## 6. Testing

1. Router test: SSE payload yields expected token sequence.
2. Progress form test: planning/finalizing token delegates append into model stream text.
3. Regression: existing gateway/executor/progress tests remain green.

