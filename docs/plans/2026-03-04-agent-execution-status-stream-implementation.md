# Agent Execution Status Stream Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add a top status strip to execution progress dialog that shows live phase and cumulative streamed token count with an adjustable state model.

**Architecture:** Introduce a small status view model for phase/token state, wire it into `AgentExecutionProgressForm`, and keep all existing transcript/tool-step behavior intact. The form only renders view-model output and updates it from already-available events.

**Tech Stack:** .NET 8, C#, Windows Forms, xUnit, FluentAssertions

---

### Task 1: Add failing tests for status phase and token count

**Files:**
- Modify: `tests/VStartNext.Core.Tests/UI/AgentExecutionProgressFormTests.cs`

**Step 1: Write the failing tests**

Add tests that assert:
- completion run updates phase to `Completed`
- planning/finalizing streams update cumulative token count
- cancel path updates phase to `Canceled`
- runner exception updates phase to `Failed`

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter FullyQualifiedName~AgentExecutionProgressFormTests -v minimal`  
Expected: FAIL with missing status/metric members.

**Step 3: Commit**

```bash
git add tests/VStartNext.Core.Tests/UI/AgentExecutionProgressFormTests.cs
git commit -m "test: add execution progress status strip expectations"
```

### Task 2: Implement status view model and form wiring

**Files:**
- Create: `src/VStartNext.App/Windows/Execution/ExecutionStatusPhase.cs`
- Create: `src/VStartNext.App/Windows/Execution/ExecutionStatusViewModel.cs`
- Modify: `src/VStartNext.App/Windows/AgentExecutionProgressForm.cs`

**Step 1: Minimal implementation**

1. Add phase enum values:
   - `Planning`, `Running`, `Finalizing`, `Completed`, `Canceled`, `Failed`
2. Add view model:
   - stores phase + token count
   - methods to transition phase and add token chunk
   - exposes display strings (phase text and token text)
3. In form:
   - add status strip controls at top
   - initialize via view model
   - update phase at planning/running/finalizing/final states
   - update token count when appending streamed token chunks
   - expose testing hooks for phase/token text/count

**Step 2: Run focused tests**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter FullyQualifiedName~AgentExecutionProgressFormTests -v minimal`  
Expected: PASS.

**Step 3: Commit**

```bash
git add src/VStartNext.App/Windows/Execution src/VStartNext.App/Windows/AgentExecutionProgressForm.cs tests/VStartNext.Core.Tests/UI/AgentExecutionProgressFormTests.cs
git commit -m "feat: add execution status strip and token counter in progress dialog"
```

### Task 3: Add unit tests for the status view model

**Files:**
- Create: `tests/VStartNext.Core.Tests/UI/ExecutionStatusViewModelTests.cs`

**Step 1: Write tests**

Cover:
- default values
- phase transitions
- non-empty token chunk accumulation
- generated display text

**Step 2: Run targeted tests**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter FullyQualifiedName~ExecutionStatusViewModelTests -v minimal`  
Expected: PASS.

**Step 3: Commit**

```bash
git add tests/VStartNext.Core.Tests/UI/ExecutionStatusViewModelTests.cs
git commit -m "test: cover execution status view model transitions"
```

### Task 4: QA gate and full verification

**Files:**
- Modify: `docs/qa/mvp-checklist.md`

**Step 1: Update QA checklist**

Add:
- `[ ] Execution progress dialog status strip reflects phase transitions and cumulative stream tokens.`

**Step 2: Run full verification**

Run:

```bash
dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj -v minimal
dotnet build VStartNext.sln -c Release
powershell -ExecutionPolicy Bypass -File scripts/verify.ps1
```

Expected:
1. tests pass
2. release build succeeds with zero errors
3. verification script prints `Verification passed`

**Step 3: Commit**

```bash
git add docs/qa/mvp-checklist.md
git commit -m "chore: add qa gate for execution status strip"
```
