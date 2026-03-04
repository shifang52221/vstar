# VStart Next Model Settings UI Design (Wave 3.2)

Date: 2026-03-04
Status: Approved
Priority: User-managed cloud model settings via in-app UI

## 1. Objective

Provide a first-class in-app settings surface for AI model configuration so users can:

1. Set provider/base URL/API key
2. Configure model routing (`chat/planner/reflection`)
3. Tune runtime parameters
4. Test provider connectivity before saving

## 2. Entry and Navigation

Entry point follows approved choice:

1. Add `AI Settings` button in right `ContextPanel`
2. Clicking opens a dedicated `ModelSettingsForm`
3. No tray-only entry in this wave

## 3. Backend and Persistence

Use the backend delivered in Wave 3.1 and extend with file-based config persistence:

1. `AppConfigFileStore` persists config JSON at `LocalAppData/VStartNext/config.json`
2. `ModelSettingsService` loads/decrypts/saves/encrypts settings
3. API key remains encrypted at rest via DPAPI
4. Existing corrupted-config fallback remains active

## 4. UI Form Fields

`ModelSettingsForm` includes:

1. `Provider` dropdown
2. `Base URL`
3. `API Key`
4. `Chat Model`
5. `Planner Model`
6. `Reflection Model`
7. `Temperature`
8. `Max Tokens`
9. `Timeout Seconds`
10. `Retry Count`

Actions:

1. `Test Connection`
2. `Save`
3. `Cancel`

## 5. Interaction Rules

1. Form opens with current persisted settings
2. `Test Connection` uses current in-form values (no forced save)
3. `Save` validates required fields and persists encrypted configuration
4. Success/failure feedback shown via message box

## 6. Runtime Behavior Scope

This wave focuses on settings ownership and secure persistence.

1. Runtime agent execution path remains compatible
2. Model router deep runtime consumption is next wave

## 7. Quality and Validation

Required tests:

1. Config file store load/save roundtrip
2. Model settings service encrypts API key on save and decrypts on load
3. Connection test pass/fail mapping
4. Context panel button event wiring

Verification gate:

1. `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj -v minimal`
2. `dotnet build VStartNext.sln -c Release`
3. `powershell -ExecutionPolicy Bypass -File scripts/verify.ps1`

