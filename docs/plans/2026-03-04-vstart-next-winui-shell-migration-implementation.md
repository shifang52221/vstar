# VStart Next WinUI Shell Migration (Wave 1) Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Introduce shell host abstraction and host-mode routing so WinUI migration can proceed incrementally while keeping WinForms as stable default.

**Architecture:** Add `IAppShellHost` and `ShellHostFactory` in `VStartNext.App`, route `Program` through the factory, and keep the existing `ShellWindowForm` behavior via a `WinFormsShellHost` adapter.

**Tech Stack:** .NET 8, C#, Windows Forms (current runtime), xUnit, FluentAssertions

---

## Plan Rules

1. Follow @test-driven-development for each behavior change.
2. Follow @verification-before-completion before completion claims.
3. Keep commits atomic and push to `feature/vstart-next-winui` and `main`.

### Task 1: Add Red Tests for Shell Host Mode Routing

**Files:**
- Create: `tests/VStartNext.Core.Tests/UI/ShellHostFactoryTests.cs`

**Step 1: Write failing tests**

```csharp
[Fact]
public void ResolveMode_DefaultsToWinForms_WhenValueMissing() { }

[Fact]
public void ResolveMode_UsesWinUiPreview_WhenConfigured() { }

[Fact]
public void ResolveMode_FallsBackToWinForms_WhenValueInvalid() { }
```

**Step 2: Run tests to confirm RED**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter FullyQualifiedName~ShellHostFactoryTests -v minimal`  
Expected: FAIL due to missing factory/mode types.

**Step 3: Commit red tests**

```bash
git add tests/VStartNext.Core.Tests/UI/ShellHostFactoryTests.cs
git commit -m "test: add shell host mode routing tests"
```

### Task 2: Implement Host Contract and Factory

**Files:**
- Create: `src/VStartNext.App/Windows/IAppShellHost.cs`
- Create: `src/VStartNext.App/Windows/ShellHostMode.cs`
- Create: `src/VStartNext.App/Windows/ShellHostFactory.cs`
- Create: `src/VStartNext.App/Windows/WinFormsShellHost.cs`

**Step 1: Minimal implementation**

1. `IAppShellHost` exposes:
   - `IShellWindow ShellWindow`
   - `IWin32Window OwnerWindow`
   - `event EventHandler<string> CommandSubmitted`
   - `void SetOpenModelSettingsHandler(Action handler)`
2. `ShellHostMode` values: `WinForms`, `WinUiPreview`.
3. `ShellHostFactory.ResolveMode(string?)` returns mode with WinForms fallback.
4. `ShellHostFactory.Create(...)` returns `WinFormsShellHost` for both modes in Wave 1 (WinUiPreview temporarily mapped to WinForms).
5. `WinFormsShellHost` wraps existing `ShellWindowForm`.

**Step 2: Run focused tests**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter FullyQualifiedName~ShellHostFactoryTests -v minimal`  
Expected: PASS.

**Step 3: Commit**

```bash
git add src/VStartNext.App/Windows tests/VStartNext.Core.Tests/UI/ShellHostFactoryTests.cs
git commit -m "feat: add shell host factory and mode abstraction"
```

### Task 3: Wire Program Through Host Factory

**Files:**
- Modify: `src/VStartNext.App/Program.cs`

**Step 1: Integrate factory**

1. Resolve mode from env var `VSTART_SHELL_MODE`.
2. Create host via factory.
3. Replace direct `ShellWindowForm` usage in `Program` with `IAppShellHost`.
4. Keep current runtime behavior unchanged for default mode.

**Step 2: Run targeted regression tests**

Run:
`dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter "FullyQualifiedName~NeoPanelInteractionFlowTests|FullyQualifiedName~ShellWindowModelSettingsTests|FullyQualifiedName~AppShellVisibilityEventsTests" -v minimal`  
Expected: PASS.

**Step 3: Commit**

```bash
git add src/VStartNext.App/Program.cs
git commit -m "feat: route app startup through shell host factory"
```

### Task 4: Verification and QA Gate Update

**Files:**
- Modify: `docs/qa/mvp-checklist.md`

**Step 1: Add QA item**

Add:
`[ ] Shell host mode routing defaults to WinForms and supports WinUI preview flag.`

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
git commit -m "chore: add qa gate for shell host mode routing"
```
