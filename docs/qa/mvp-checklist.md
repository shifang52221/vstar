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

## Stability

- [ ] App starts without crash and stays resident in tray.
- [ ] Config corruption fallback restores defaults.
- [ ] Verify script completes successfully.
- [ ] Shell visibility interaction remains consistent after command submissions.
- [ ] High-risk agent actions require confirmation.
- [ ] Bilingual response policy resolves by input language by default.
- [ ] Agent memory profile stores language and tool usage frequency.

## Release Gate

- [ ] `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter Agent -v minimal`
- [ ] `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter "FullyQualifiedName~AppAgentRoutingTests|FullyQualifiedName~NeoPanelInteractionFlowTests" -v minimal`
- [ ] `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter NeoPanel -v minimal`
- [ ] `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter "FullyQualifiedName~CommandPrefixParserTests|FullyQualifiedName~CommandPaletteServiceTests" -v minimal`
- [ ] `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter FullyQualifiedName~FlowLaunchRunnerTests -v minimal`
- [ ] `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter "FullyQualifiedName~NeoPanelInteractionFlowTests|FullyQualifiedName~AppShellVisibilityEventsTests" -v minimal`
- [ ] `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj -v minimal`
- [ ] `dotnet build VStartNext.sln -c Release`
- [ ] `powershell -ExecutionPolicy Bypass -File scripts/verify.ps1`
