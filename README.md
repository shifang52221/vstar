## VStart Next (Windows Prototype)
This repository now also contains a new Windows launcher prototype at `src/VStartNext.App`.

### Neo-Panel Wave 2 Highlights
- Five-zone neo-panel shell layout (command bar, nav rail, launch grid, context panel, status strip).
- Command Palette+ prefixes: `calc:`, `url:`, `ws:`.
- Smart ranking upgrades with time-of-day affinity.
- Flow launch pipeline for ordered multi-step launches.
- Launch context actions (`Run as admin`, `Open directory`, `Copy path`).
- End-to-end command submission wiring from shell command bar to app orchestration state.

### AI Agent Wave 3 Highlights
- Cloud-first launcher agent contracts and execution pipeline.
- Tool registry and policy guard with high-risk confirmation gate.
- Multi-step orchestrator with reflection hook before execution.
- Bilingual response policy (`zh-CN` / `en-US`, input-language-first).
- App-level natural-language routing through an agent gateway.
- Memory profile baseline for language and tool usage frequency.

### User-Defined Model Config (Wave 3.1)
- Config schema supports provider, base URL, model routes, and runtime parameters.
- API key is stored as encrypted ciphertext (Windows DPAPI).
- OpenAI-compatible provider connection test capability (`GET /models`).
- Model config parser supports string enum provider values for human-editable configs.

### Model Settings UI (Wave 3.2)
- `AI Settings` entry added to context panel for direct in-shell access.
- Dedicated model settings form supports provider/base-url/api-key/model route editing.
- Save path uses local config file store and encrypted API key persistence.
- Test Connection action validates user-entered provider settings before save.

### Quick Verification
- dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter "FullyQualifiedName~AppConfigFileStoreTests|FullyQualifiedName~ModelSettingsServiceTests" -v minimal
- dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter "FullyQualifiedName~ContextPanelControlTests|FullyQualifiedName~ShellWindowModelSettingsTests" -v minimal
- dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter "FullyQualifiedName~AppConfigModelSettingsTests|FullyQualifiedName~ModelConfigParseTests" -v minimal
- dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter "FullyQualifiedName~DpapiSecretProtectorTests|FullyQualifiedName~ModelConnectionTesterTests" -v minimal
- dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter Agent -v minimal
- dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter "FullyQualifiedName~AppAgentRoutingTests|FullyQualifiedName~NeoPanelInteractionFlowTests" -v minimal
- dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter NeoPanel -v minimal
- dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter "FullyQualifiedName~CommandPrefixParserTests|FullyQualifiedName~CommandPaletteServiceTests" -v minimal
- dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter FullyQualifiedName~FlowLaunchRunnerTests -v minimal
- dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter "FullyQualifiedName~NeoPanelInteractionFlowTests|FullyQualifiedName~AppShellVisibilityEventsTests" -v minimal
- dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj -v minimal
- dotnet build VStartNext.sln -c Release
- powershell -ExecutionPolicy Bypass -File scripts/verify.ps1

