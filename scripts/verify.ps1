$ErrorActionPreference = "Stop"

dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter "FullyQualifiedName~AppConfigFileStoreTests|FullyQualifiedName~ModelSettingsServiceTests" -v minimal
if ($LASTEXITCODE -ne 0) { throw "Model settings persistence tests failed" }

dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter "FullyQualifiedName~ContextPanelControlTests|FullyQualifiedName~ShellWindowModelSettingsTests" -v minimal
if ($LASTEXITCODE -ne 0) { throw "Model settings ui wiring tests failed" }

dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter "FullyQualifiedName~AppAgentGatewayTests|FullyQualifiedName~OpenAiCompatibleAgentModelRouterTests|FullyQualifiedName~OpenAiCompatibleAgentPlannerTests|FullyQualifiedName~LauncherAgentToolTests" -v minimal
if ($LASTEXITCODE -ne 0) { throw "Runtime model routing tests failed" }

dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter "FullyQualifiedName~AppConfigModelSettingsTests|FullyQualifiedName~ModelConfigParseTests" -v minimal
if ($LASTEXITCODE -ne 0) { throw "Model config tests failed" }

dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter "FullyQualifiedName~DpapiSecretProtectorTests|FullyQualifiedName~ModelConnectionTesterTests" -v minimal
if ($LASTEXITCODE -ne 0) { throw "Model security/connection tests failed" }

dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter Agent -v minimal
if ($LASTEXITCODE -ne 0) { throw "Agent tests failed" }

dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter "FullyQualifiedName~AppAgentRoutingTests|FullyQualifiedName~NeoPanelInteractionFlowTests" -v minimal
if ($LASTEXITCODE -ne 0) { throw "Agent routing interaction tests failed" }

dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter NeoPanel -v minimal
if ($LASTEXITCODE -ne 0) { throw "Neo-panel tests failed" }

dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter "FullyQualifiedName~CommandPrefixParserTests|FullyQualifiedName~CommandPaletteServiceTests" -v minimal
if ($LASTEXITCODE -ne 0) { throw "Command prefix behavior tests failed" }

dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter FullyQualifiedName~FlowLaunchRunnerTests -v minimal
if ($LASTEXITCODE -ne 0) { throw "Flow launch behavior tests failed" }

dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter "FullyQualifiedName~NeoPanelInteractionFlowTests|FullyQualifiedName~AppShellVisibilityEventsTests" -v minimal
if ($LASTEXITCODE -ne 0) { throw "Visibility interaction behavior tests failed" }

dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj -v minimal
dotnet build VStartNext.sln -c Release

Write-Host "Verification passed"
