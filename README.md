# EVplayer2Crack
EV加密播放器2 反录屏、截图破解
（这段话编辑于2024年9月，此项目为2022年编写的代码，当前版本可能不适用，仅作存档学习使用）

## 原理 
APIHOOK 两个函数 SetWindowDisplayAffinity 和 OpenProcess 

## 使用方法
1.下载源码编译，或者下载Release中的成品  
2.然后安装 EVPlayer2(https://www.ieway.cn/evplayer2.html)
3.运行EVPlayer2 （进程名为 EVPlayer2.exe）  
4.打开我的软件，提示  
注入成功，破解成功 即可进行截图，录屏

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

### Quick Verification
- dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter Agent -v minimal
- dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter "FullyQualifiedName~AppAgentRoutingTests|FullyQualifiedName~NeoPanelInteractionFlowTests" -v minimal
- dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter NeoPanel -v minimal
- dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter "FullyQualifiedName~CommandPrefixParserTests|FullyQualifiedName~CommandPaletteServiceTests" -v minimal
- dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter FullyQualifiedName~FlowLaunchRunnerTests -v minimal
- dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter "FullyQualifiedName~NeoPanelInteractionFlowTests|FullyQualifiedName~AppShellVisibilityEventsTests" -v minimal
- dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj -v minimal
- dotnet build VStartNext.sln -c Release
- powershell -ExecutionPolicy Bypass -File scripts/verify.ps1

