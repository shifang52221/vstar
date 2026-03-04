# VStart Next User-Defined Model Config Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Implement backend capabilities so users can configure their own cloud model provider settings securely (including encrypted API key and connection test).

**Architecture:** Extend `AppConfig` with AI provider settings in `VStartNext.Core`, add DPAPI secret protection and provider connection test in `VStartNext.Infrastructure`, and preserve existing launcher behavior.

**Tech Stack:** .NET 8, C#, System.Text.Json, Windows DPAPI (`ProtectedData`), HttpClient, xUnit, FluentAssertions

---

## Plan Rules

1. Follow @test-driven-development for each behavior change.
2. Keep one atomic commit per task.
3. Run targeted tests first, then full test suite.

### Task 1: AI Model Settings Schema in AppConfig

**Files:**
- Modify: `src/VStartNext.Core/Config/AppConfig.cs`
- Create: `src/VStartNext.Core/Config/AiProviderKind.cs`
- Create: `src/VStartNext.Core/Config/AiModelSettings.cs`
- Create: `src/VStartNext.Core/Config/AiModelRouteSettings.cs`
- Test: `tests/VStartNext.Core.Tests/Config/AppConfigModelSettingsTests.cs`

**Step 1: Write failing tests**
- default config should include non-null model settings
- route slots should have defaults (`chat/planner/reflection`)

**Step 2: Verify RED**
Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter AppConfigModelSettings -v minimal`

**Step 3: Minimal implementation**
- add typed settings classes and defaults
- include in `AppConfig.Default()`

**Step 4: Verify GREEN**
Run same filter command, expect PASS.

**Step 5: Commit**
`feat: add user model settings schema to app config`

### Task 2: Config Parse Compatibility for Model Settings

**Files:**
- Modify: `tests/VStartNext.Core.Tests/Config/ConfigRecoveryTests.cs`
- Create: `tests/VStartNext.Core.Tests/Config/ModelConfigParseTests.cs`

**Step 1: Write failing test**
- parse JSON containing model config and assert values survive deserialize

**Step 2: Verify RED**
Run: `dotnet test ... --filter ModelConfigParseTests -v minimal`

**Step 3: Minimal implementation**
- adjust config model if needed for deserialization compatibility

**Step 4: Verify GREEN**
Run same filter command, expect PASS.

**Step 5: Commit**
`test: cover model settings parse compatibility`

### Task 3: DPAPI Secret Protection Service

**Files:**
- Create: `src/VStartNext.Infrastructure/Security/ISecretProtector.cs`
- Create: `src/VStartNext.Infrastructure/Security/DpapiSecretProtector.cs`
- Test: `tests/VStartNext.Core.Tests/Security/DpapiSecretProtectorTests.cs`

**Step 1: Write failing test**
- protect+unprotect round trip returns original plaintext

**Step 2: Verify RED**
Run: `dotnet test ... --filter DpapiSecretProtectorTests -v minimal`

**Step 3: Minimal implementation**
- DPAPI `CurrentUser` scope
- base64 ciphertext output

**Step 4: Verify GREEN**
Run same filter command, expect PASS.

**Step 5: Commit**
`feat: add dpapi secret protection service`

### Task 4: OpenAI-Compatible Connection Tester

**Files:**
- Create: `src/VStartNext.Infrastructure/AI/ModelConnectionTestRequest.cs`
- Create: `src/VStartNext.Infrastructure/AI/ModelConnectionTestResult.cs`
- Create: `src/VStartNext.Infrastructure/AI/ModelConnectionTester.cs`
- Test: `tests/VStartNext.Core.Tests/AI/ModelConnectionTesterTests.cs`

**Step 1: Write failing tests**
- success when API returns 200
- failure returns status/message when API returns 401

**Step 2: Verify RED**
Run: `dotnet test ... --filter ModelConnectionTesterTests -v minimal`

**Step 3: Minimal implementation**
- GET `{baseUrl}/models`
- bearer header
- structured result

**Step 4: Verify GREEN**
Run same filter command, expect PASS.

**Step 5: Commit**
`feat: add openai-compatible model connection tester`

### Task 5: Verification and Docs Refresh

**Files:**
- Modify: `scripts/verify.ps1`
- Modify: `docs/qa/mvp-checklist.md`
- Modify: `README.md`

**Step 1: Add model-config test gates**
- include filters for config/security/connection tests

**Step 2: Verify**
Run:
- `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj -v minimal`
- `dotnet build VStartNext.sln -c Release`
- `powershell -ExecutionPolicy Bypass -File scripts/verify.ps1`

**Step 3: Commit**
`chore: refresh verification and docs for user model config`

## Final Verification Gate

Run:

```bash
dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj -v minimal
dotnet build VStartNext.sln -c Release
powershell -ExecutionPolicy Bypass -File scripts/verify.ps1
```

