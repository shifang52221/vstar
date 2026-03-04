# VStart Next Model Settings UI Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add an in-app AI settings UI so users can manage provider/model configuration securely and test connectivity.

**Architecture:** Add persistence and service layers in `Infrastructure`/`App`, expose an `AI Settings` entry in right context panel, and launch a WinForms settings dialog with encrypted save and live connection test.

**Tech Stack:** .NET 8, C#, Windows Forms, System.Text.Json, DPAPI, HttpClient, xUnit, FluentAssertions

---

## Plan Rules

1. Follow @test-driven-development for each behavior change.
2. Keep one atomic commit per task.
3. Run targeted test then full test suite for each task.

### Task 1: AppConfig File Store

**Files:**
- Create: `src/VStartNext.Infrastructure/Storage/AppConfigFileStore.cs`
- Test: `tests/VStartNext.Core.Tests/Config/AppConfigFileStoreTests.cs`

**Steps (RED -> GREEN -> COMMIT):**

1. Failing tests for:
- missing file returns default config
- save then load roundtrip preserves `ModelSettings.BaseUrl`
2. Implement minimal file store with default path and optional path override.
3. Verify targeted and full tests.
4. Commit: `feat: add app config file store`

### Task 2: Model Settings Service

**Files:**
- Create: `src/VStartNext.App/Settings/ModelSettingsInput.cs`
- Create: `src/VStartNext.App/Settings/IModelSettingsService.cs`
- Create: `src/VStartNext.App/Settings/ModelSettingsService.cs`
- Test: `tests/VStartNext.Core.Tests/UI/ModelSettingsServiceTests.cs`

**Steps:**

1. Failing tests for:
- save encrypts API key in persisted config
- load decrypts API key for UI model
2. Implement minimal service using `AppConfigFileStore` + `ISecretProtector` + `ModelConnectionTester`.
3. Verify tests.
4. Commit: `feat: add model settings service for encrypted load save`

### Task 3: ContextPanel AI Settings Entry

**Files:**
- Modify: `src/VStartNext.App/Windows/Controls/ContextPanelControl.cs`
- Test: `tests/VStartNext.Core.Tests/UI/ContextPanelControlTests.cs`

**Steps:**

1. Failing test that clicking AI settings button raises an event.
2. Implement button + event.
3. Verify tests.
4. Commit: `feat: add context panel ai settings entry`

### Task 4: NeoPanel and Shell Event Wiring

**Files:**
- Modify: `src/VStartNext.App/Windows/NeoPanelView.cs`
- Modify: `src/VStartNext.App/Windows/ShellWindowForm.cs`
- Test: `tests/VStartNext.Core.Tests/UI/NeoPanelCompositionTests.cs`

**Steps:**

1. Add failing assertion for AI settings entry presence/event bridge.
2. Implement event propagation from `ContextPanelControl` -> `NeoPanelView` -> `ShellWindowForm`.
3. Verify tests.
4. Commit: `feat: wire ai settings entry through shell view hierarchy`

### Task 5: Model Settings Form UI and Open Flow

**Files:**
- Create: `src/VStartNext.App/Windows/ModelSettingsForm.cs`
- Modify: `src/VStartNext.App/Program.cs`
- Modify: `src/VStartNext.App/Windows/ShellWindowForm.cs`
- Test: `tests/VStartNext.Core.Tests/UI/ShellWindowModelSettingsTests.cs`

**Steps:**

1. Failing test for shell invoking model settings open flow.
2. Implement form with fields + buttons (`Test Connection`, `Save`, `Cancel`).
3. Wire shell event to open dialog via injected factory/callback.
4. Verify tests.
5. Commit: `feat: add model settings form and open flow wiring`

### Task 6: Verification and Docs Refresh

**Files:**
- Modify: `scripts/verify.ps1`
- Modify: `docs/qa/mvp-checklist.md`
- Modify: `README.md`

**Steps:**

1. Add verify filters for new config store/service/ui wiring tests.
2. Update docs with AI settings UI capability and commands.
3. Run full gate:
- `dotnet test ...`
- `dotnet build ...`
- `powershell -ExecutionPolicy Bypass -File scripts/verify.ps1`
4. Commit: `chore: refresh verification and docs for model settings ui`

## Final Verification Gate

Run:

```bash
dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj -v minimal
dotnet build VStartNext.sln -c Release
powershell -ExecutionPolicy Bypass -File scripts/verify.ps1
```

