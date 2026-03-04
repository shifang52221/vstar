# VStart Next Token Streaming Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add OpenAI-compatible SSE token streaming to model lane for planning and finalizing phases without breaking existing execution behavior.

**Architecture:** Extend model-router interface with a streaming method, implement SSE parsing in infrastructure router, and wire optional planning/finalizing stream delegates into execution progress form from `Program`.

**Tech Stack:** .NET 8, C#, Windows Forms, xUnit, FluentAssertions

---

## Plan Rules

1. Follow @test-driven-development for each behavior change.
2. Follow @verification-before-completion before completion claims.
3. Keep one atomic feature commit.

### Task 1: Add Red Tests for Streaming Contract and UI Rendering

**Files:**
- Modify: `tests/VStartNext.Core.Tests/AI/OpenAiCompatibleAgentModelRouterTests.cs`
- Modify: `tests/VStartNext.Core.Tests/UI/AgentExecutionProgressFormTests.cs`

**Step 1: Write failing router stream test**

```csharp
[Fact]
public async Task StreamCompletionAsync_ParsesSseTokensInOrder()
{
    // feed SSE lines with delta.content tokens
    // expect returned token list equals ["Hel", "lo"]
}
```

**Step 2: Write failing progress form token test**

```csharp
[Fact]
public async Task RunForTestingAsync_WithPlanningAndFinalizingStreams_AppendsTokens()
{
    // inject planning/finalizing token delegates
    // expect model stream text contains token fragments
}
```

**Step 3: Run focused tests and confirm RED**

Run:
`dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter "FullyQualifiedName~OpenAiCompatibleAgentModelRouterTests|FullyQualifiedName~AgentExecutionProgressFormTests" -v minimal`

Expected: fail due to missing streaming method / form hooks.

### Task 2: Implement Streaming Contract + Router SSE Parsing

**Files:**
- Modify: `src/VStartNext.Core/Agent/IAgentModelRouter.cs`
- Modify: `src/VStartNext.Infrastructure/AI/OpenAiCompatibleAgentModelRouter.cs`
- Modify: `tests/VStartNext.Core.Tests/AI/OpenAiCompatibleAgentPlannerTests.cs`
- Modify: `tests/VStartNext.Core.Tests/UI/AppAgentGatewayTests.cs`

**Step 1: Implement interface + router stream**

1. Add `StreamCompletionAsync`.
2. Send `stream=true` request.
3. Parse SSE `data:` lines and yield `delta.content` tokens.
4. Keep non-stream `CompleteAsync` unchanged.

**Step 2: Update test doubles implementing `IAgentModelRouter`**

Add trivial stream implementation yielding full completion once.

**Step 3: Run focused tests and confirm GREEN**

Run:
`dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter "FullyQualifiedName~OpenAiCompatibleAgentModelRouterTests|FullyQualifiedName~OpenAiCompatibleAgentPlannerTests|FullyQualifiedName~AppAgentGatewayTests" -v minimal`

Expected: pass.

### Task 3: Wire Planning/Finalizing Token Streams into Progress Dialog

**Files:**
- Modify: `src/VStartNext.App/Windows/AgentExecutionProgressForm.cs`
- Modify: `src/VStartNext.App/Program.cs`
- Modify: `tests/VStartNext.Core.Tests/UI/AgentExecutionProgressFormTests.cs`

**Step 1: Implement optional planning/finalizing stream delegates**

1. Extend progress form constructor with optional token stream factories.
2. Render phase header + token fragments in model lane.
3. Add fallback line if stream unavailable.
4. Respect cancellation token.

**Step 2: Program wiring**

1. Pass model router into `ShowAgentExecutionProgress`.
2. Build planning/finalizing prompts from preview and run result.
3. Provide stream delegates to form.

**Step 3: Run focused tests**

Run:
`dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter "FullyQualifiedName~AgentExecutionProgressFormTests|FullyQualifiedName~AppAgentGatewayTests|FullyQualifiedName~AgentExecutorTests" -v minimal`

Expected: pass.

### Task 4: Full Verification and Commit

**Files:**
- Modify: `docs/qa/mvp-checklist.md` (if needed)

**Step 1: Full verification**

Run:

```bash
dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj -v minimal
dotnet build VStartNext.sln -c Release
powershell -ExecutionPolicy Bypass -File scripts/verify.ps1
```

Expected:

1. tests pass
2. build passes
3. verify script outputs `Verification passed`

**Step 2: Commit**

```bash
git add .
git commit -m "feat: add model token streaming for planning and finalizing phases"
```

