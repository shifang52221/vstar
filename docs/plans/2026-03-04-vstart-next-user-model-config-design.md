# VStart Next User-Defined Model Config Design (Wave 3.1)

Date: 2026-03-04
Status: Approved
Priority: User-owned AI model configuration and secure secret storage

## 1. Objective

Ensure model selection is controlled by the user, not hardcoded by the app.

The first deliverable (without UI page) includes:

1. Config schema for provider/base-url/model-routing/parameters
2. Secure API key protection using Windows DPAPI
3. Connection test capability for user-provided provider config

## 2. Strategy

Build backend-first infrastructure and keep UI-independent contracts.

Why:

1. Avoid rework when settings page is introduced
2. Enable immediate integration with agent routing and model router
3. Preserve backward compatibility for existing config behavior

## 3. Configuration Model

Extend `AppConfig` with AI model settings:

1. Provider kind (`OpenAICompatible` baseline)
2. Base URL (user-specified)
3. Encrypted API key payload (persisted ciphertext only)
4. Model routing slots (`chat`, `planner`, `reflection`)
5. Tunables (`temperature`, `maxTokens`, `timeoutSeconds`, `retryCount`)

Defaults:

1. Cloud-first profile
2. Safe latency-oriented defaults
3. Explicit schema version retention

## 4. Secret Security

Use Windows DPAPI for local encryption/decryption.

Rules:

1. Never persist plaintext API key to config file
2. Encryption/decryption is machine+user scoped
3. Fail closed on malformed ciphertext

## 5. Connection Test

Add provider test service for OpenAI-compatible endpoints.

Flow:

1. Validate required fields
2. Build `GET {baseUrl}/models` request
3. Send bearer-auth request
4. Return structured result (`success`, `statusCode`, `message`)

## 6. Integration Boundaries (Current Wave)

In this increment:

1. Provide contracts and infra implementation
2. Keep Agent runtime compatible
3. Do not build settings UI yet

## 7. Testing and Validation

Required coverage:

1. Config defaults include model settings
2. Config parse keeps model settings values
3. DPAPI protector round-trip success
4. Connection tester sends expected request and handles failure status

Validation gate:

1. `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj -v minimal`
2. `dotnet build VStartNext.sln -c Release`
3. `powershell -ExecutionPolicy Bypass -File scripts/verify.ps1`

