# VStart Next Dual-Stream Execution Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add a dual-stream agent execution UI that shows model-status text and tool-step trace in real time, with one-click cancel reflected in both streams.

**Architecture:** Keep core execution contract stable and implement dual-stream behavior mainly in `AgentExecutionProgressForm`. Reuse current progress callback path from `AppAgentGateway` and map lifecycle/tool events into separate UI lanes.

**Tech Stack:** .NET 8, C#, Windows Forms, xUnit, FluentAssertions

---

## Plan Rules

1. Follow @test-driven-development for every behavior change.
2. Follow @verification-before-completion before completion claims.
3. Use one atomic commit for this feature.

### Task 1: Add Failing Progress-Form Tests for Dual Stream

**Files:**
- Modify: `tests/VStartNext.Core.Tests/UI/AgentExecutionProgressFormTests.cs`

**Step 1: Write the failing test**

```csharp
[Fact]
public async Task RunForTestingAsync_AppendsModelAndToolStreams()
{
    // create form with runner reporting two tool updates
    // assert model lines include lifecycle and tool-derived lines
    // assert tool count is 2
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter FullyQualifiedName~AgentExecutionProgressFormTests -v minimal`  
Expected: FAIL due to missing test helpers/dual-stream behavior.

**Step 3: Write minimal implementation support in test (if needed)**

Add assertions using public test hooks to inspect:

1. model stream lines
2. tool stream update count

**Step 4: Run test to verify it still fails for expected reason**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter FullyQualifiedName~AgentExecutionProgressFormTests -v minimal`  
Expected: FAIL because production form does not yet provide dual-stream updates.

**Step 5: Commit**

```bash
git add tests/VStartNext.Core.Tests/UI/AgentExecutionProgressFormTests.cs
git commit -m "test: add red tests for dual-stream execution form"
```

### Task 2: Implement Dual-Stream UI and Lifecycle Mapping

**Files:**
- Modify: `src/VStartNext.App/Windows/AgentExecutionProgressForm.cs`

**Step 1: Write minimal implementation**

Implement:

1. top model stream lane (`TextBox` multiline, read-only)
2. bottom tool stream lane (`ListView`)
3. model lifecycle lines:
   - execution started
   - step running/succeeded/failed
   - canceled/succeeded/failed end state
4. test hooks for:
   - running execution directly in tests
   - reading model lines
   - reading tool update count

**Step 2: Run focused tests**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter FullyQualifiedName~AgentExecutionProgressFormTests -v minimal`  
Expected: PASS.

**Step 3: Commit**

```bash
git add src/VStartNext.App/Windows/AgentExecutionProgressForm.cs tests/VStartNext.Core.Tests/UI/AgentExecutionProgressFormTests.cs
git commit -m "feat: add dual-stream agent execution progress ui"
```

### Task 3: Verify Integration and Regression Safety

**Files:**
- Modify: `docs/qa/mvp-checklist.md` (if new behavior needs explicit checklist item)

**Step 1: Run integration-targeted tests**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter "FullyQualifiedName~AppAgentGatewayTests|FullyQualifiedName~AgentExecutorTests|FullyQualifiedName~AgentExecutionProgressFormTests" -v minimal`  
Expected: PASS.

**Step 2: Run full project verification**

Run:

```bash
dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj -v minimal
dotnet build VStartNext.sln -c Release
powershell -ExecutionPolicy Bypass -File scripts/verify.ps1
```

Expected:

1. all tests pass
2. release build succeeds with zero errors
3. verification script prints `Verification passed`

**Step 3: Commit (if checklist/doc changed)**

```bash
git add docs/qa/mvp-checklist.md
git commit -m "chore: update qa checklist for dual-stream execution"
```

