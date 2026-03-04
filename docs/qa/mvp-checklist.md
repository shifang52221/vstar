# VStart Next MVP QA Checklist

## Core Flow

- [ ] Alt+Space toggles launcher visibility.
- [ ] Search returns browser apps for `ch` query.
- [ ] Launching a missing target shows readable error.
- [ ] Pinned items rank above non-pinned in catalog list.
- [ ] `calc:` command returns computed output in command bar flow.
- [ ] `url:` and `ws:` commands route to open-target execution.
- [ ] Flow launch executes configured steps in deterministic order.
- [ ] Neo-panel command submission updates app result state.
- [ ] Natural-language command routes through app agent gateway.
- [ ] Agent orchestrator invokes reflection before execution.
- [ ] Agent executor executes multi-step plans in declared order.
- [ ] User can define provider/base-url/model routes in config.
- [ ] String enum provider values deserialize without recovery fallback.
- [ ] Context panel exposes `AI Settings` entry.
- [ ] Shell open flow can trigger model settings dialog callback.
- [ ] App agent gateway uses model router output for NL commands.
- [ ] Agent gateway can execute orchestrator tool-chain output.
- [ ] Planner can parse model JSON response into executable tool steps.
- [ ] Runtime launcher tools can execute app/url/path actions safely.
- [ ] Agent output includes step-by-step execution trace in UI result text.
- [ ] Agent completion summary follows input language by default.
- [ ] Preview panel supports execute-all / single-step / cancel decisions.
- [ ] Single-step execution mode only runs first planned action.
- [ ] Agent audit store persists recent execution records.
- [ ] Execution progress dialog shows model-status stream and tool-step stream with cancel feedback.
- [ ] Execution dialog planning/finalizing lanes support cloud token streaming with fallback.
- [ ] Planning lane replays token stream captured from real planner generation.

## Stability

- [ ] App starts without crash and stays resident in tray.
- [ ] Config corruption fallback restores defaults.
- [ ] Verify script completes successfully.
- [ ] Shell visibility interaction remains consistent after command submissions.
- [ ] High-risk agent actions require confirmation.
- [ ] Bilingual response policy resolves by input language by default.
- [ ] Agent memory profile stores language and tool usage frequency.
- [ ] API key encryption/decryption round-trip works via DPAPI.
- [ ] Provider connection test returns structured status on success/failure.
- [ ] Model settings service can save/load encrypted values through file store.
- [ ] OpenAI-compatible runtime router reads persisted model configuration.

## Release Gate

- [ ] `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter "FullyQualifiedName~AppConfigFileStoreTests|FullyQualifiedName~ModelSettingsServiceTests" -v minimal`
- [ ] `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter "FullyQualifiedName~ContextPanelControlTests|FullyQualifiedName~ShellWindowModelSettingsTests" -v minimal`
- [ ] `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter "FullyQualifiedName~AppAgentGatewayTests|FullyQualifiedName~OpenAiCompatibleAgentModelRouterTests|FullyQualifiedName~OpenAiCompatibleAgentPlannerTests|FullyQualifiedName~LauncherAgentToolTests|FullyQualifiedName~AgentExecutionPreviewFormTests|FullyQualifiedName~AgentAuditFileStoreTests" -v minimal`
- [ ] `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter "FullyQualifiedName~AppConfigModelSettingsTests|FullyQualifiedName~ModelConfigParseTests" -v minimal`
- [ ] `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter "FullyQualifiedName~DpapiSecretProtectorTests|FullyQualifiedName~ModelConnectionTesterTests" -v minimal`
- [ ] `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter Agent -v minimal`
- [ ] `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter "FullyQualifiedName~AppAgentRoutingTests|FullyQualifiedName~NeoPanelInteractionFlowTests" -v minimal`
- [ ] `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter NeoPanel -v minimal`
- [ ] `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter "FullyQualifiedName~CommandPrefixParserTests|FullyQualifiedName~CommandPaletteServiceTests" -v minimal`
- [ ] `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter FullyQualifiedName~FlowLaunchRunnerTests -v minimal`
- [ ] `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter "FullyQualifiedName~NeoPanelInteractionFlowTests|FullyQualifiedName~AppShellVisibilityEventsTests" -v minimal`
- [ ] `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj -v minimal`
- [ ] `dotnet build VStartNext.sln -c Release`
- [ ] `powershell -ExecutionPolicy Bypass -File scripts/verify.ps1`
