# VStart Next Neo-Panel Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Build the Wave 2 Neo-Panel release that exceeds legacy Vstart in both visual quality and functional capability (command palette+, smart ranking 2.0, flow launch, context actions).

**Architecture:** Keep current layered structure and add a dedicated Neo-Panel UI layer in `VStartNext.App.Windows` plus command/ranking services in `VStartNext.Core`. Route user input through command bar orchestration, then dispatch actions through launch pipeline. Preserve hotkey/tray shell lifecycle and attach richer state providers incrementally.

**Tech Stack:** .NET 8, C#, Windows Forms shell host, existing Core/Infrastructure projects, xUnit, FluentAssertions

---

## Plan Rules

1. Follow @test-driven-development for every behavior change.
2. Use @verification-before-completion before claiming any task complete.
3. Keep one atomic commit per task.

### Task 1: Neo-Panel Layout Host

**Files:**
- Create: `src/VStartNext.App/Windows/NeoPanelView.cs`
- Modify: `src/VStartNext.App/Windows/ShellWindowForm.cs`
- Test: `tests/VStartNext.Core.Tests/UI/NeoPanelViewTests.cs`

**Step 1: Write the failing test**

```csharp
using FluentAssertions;
using VStartNext.App.Windows;
using Xunit;

public class NeoPanelViewTests
{
    [Fact]
    public void NeoPanel_HasFiveZones()
    {
        var view = new NeoPanelView();
        view.ZoneCount.Should().Be(5);
    }
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter NeoPanel_HasFiveZones -v minimal`  
Expected: FAIL with missing `NeoPanelView`.

**Step 3: Write minimal implementation**

```csharp
public sealed class NeoPanelView : UserControl
{
    public int ZoneCount => 5;
}
```

Wire the control into `ShellWindowForm`.

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter NeoPanel_HasFiveZones -v minimal`  
Expected: PASS.

**Step 5: Commit**

```bash
git add src/VStartNext.App/Windows/ShellWindowForm.cs src/VStartNext.App/Windows/NeoPanelView.cs tests/VStartNext.Core.Tests/UI/NeoPanelViewTests.cs
git commit -m "feat: add neo-panel five-zone shell layout host"
```

### Task 2: Command Palette+ Prefix Parser

**Files:**
- Create: `src/VStartNext.Core/Search/CommandPrefixParser.cs`
- Create: `src/VStartNext.Core/Search/CommandPrefixType.cs`
- Test: `tests/VStartNext.Core.Tests/Search/CommandPrefixParserTests.cs`

**Step 1: Write the failing test**

```csharp
[Fact]
public void Parse_CalcPrefix_Detected()
{
    var parser = new CommandPrefixParser();
    var result = parser.Parse("calc: 2*36");
    result.Type.Should().Be(CommandPrefixType.Calc);
    result.Payload.Should().Be("2*36");
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter Parse_CalcPrefix_Detected -v minimal`  
Expected: FAIL with missing parser classes.

**Step 3: Write minimal implementation**

```csharp
public enum CommandPrefixType { None, Ws, Url, Calc }
public sealed record ParsedPrefix(CommandPrefixType Type, string Payload);
```

```csharp
public sealed class CommandPrefixParser
{
    public ParsedPrefix Parse(string input) { ... }
}
```

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter Parse_CalcPrefix_Detected -v minimal`  
Expected: PASS.

**Step 5: Commit**

```bash
git add src/VStartNext.Core/Search tests/VStartNext.Core.Tests/Search/CommandPrefixParserTests.cs
git commit -m "feat: add command palette plus prefix parser"
```

### Task 3: Command Palette Actions (`ws/url/calc`)

**Files:**
- Create: `src/VStartNext.Core/Search/ICommandActionExecutor.cs`
- Create: `src/VStartNext.Core/Search/CommandPaletteService.cs`
- Test: `tests/VStartNext.Core.Tests/Search/CommandPaletteServiceTests.cs`

**Step 1: Write the failing test**

```csharp
[Fact]
public async Task Execute_Calc_ReturnsComputedText()
{
    var service = new CommandPaletteService(new FakeExecutor());
    var result = await service.ExecuteAsync("calc: 2*36");
    result.DisplayText.Should().Be("72");
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter Execute_Calc_ReturnsComputedText -v minimal`  
Expected: FAIL with missing command palette service.

**Step 3: Write minimal implementation**

Implement routing:
- `calc:` evaluate simple arithmetic expression
- `url:` dispatch URL open request
- `ws:` map to URL open request

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter Execute_Calc_ReturnsComputedText -v minimal`  
Expected: PASS.

**Step 5: Commit**

```bash
git add src/VStartNext.Core/Search tests/VStartNext.Core.Tests/Search/CommandPaletteServiceTests.cs
git commit -m "feat: add command palette action execution for ws url calc"
```

### Task 4: Smart Ranking 2.0 (Time-of-Day Affinity)

**Files:**
- Modify: `src/VStartNext.Core/Search/SearchItem.cs`
- Modify: `src/VStartNext.Core/Search/SearchEngine.cs`
- Test: `tests/VStartNext.Core.Tests/Search/SearchRankingTimeAffinityTests.cs`

**Step 1: Write the failing test**

```csharp
[Fact]
public void Rank_PrefersItemWithMatchingTimeAffinity()
{
    var engine = new SearchEngine(new FixedClock(new DateTimeOffset(2026,3,4,9,0,0,TimeSpan.Zero)));
    var top = engine.Rank("ch", [morningItem, neutralItem]).First();
    top.Name.Should().Be("MorningBrowser");
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter Rank_PrefersItemWithMatchingTimeAffinity -v minimal`  
Expected: FAIL due to missing time affinity logic.

**Step 3: Write minimal implementation**

Add optional time affinity metadata and scoring delta by clock bucket.

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter Rank_PrefersItemWithMatchingTimeAffinity -v minimal`  
Expected: PASS.

**Step 5: Commit**

```bash
git add src/VStartNext.Core/Search tests/VStartNext.Core.Tests/Search/SearchRankingTimeAffinityTests.cs
git commit -m "feat: upgrade ranking with time-of-day affinity"
```

### Task 5: Flow Launch Model and Runner

**Files:**
- Create: `src/VStartNext.Core/Launch/FlowLaunchDefinition.cs`
- Create: `src/VStartNext.Core/Launch/FlowLaunchStep.cs`
- Create: `src/VStartNext.Infrastructure/Launch/FlowLaunchRunner.cs`
- Test: `tests/VStartNext.Core.Tests/Launch/FlowLaunchRunnerTests.cs`

**Step 1: Write the failing test**

```csharp
[Fact]
public async Task Run_ExecutesStepsInOrder()
{
    var runner = new FlowLaunchRunner(fakeExecutor);
    await runner.RunAsync(flow);
    fakeExecutor.ExecutedTargets.Should().Equal("browser.exe", "https://a.com", "C:\\Work");
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter Run_ExecutesStepsInOrder -v minimal`  
Expected: FAIL with missing flow runner.

**Step 3: Write minimal implementation**

Implement ordered step execution with per-step launch delegation.

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter Run_ExecutesStepsInOrder -v minimal`  
Expected: PASS.

**Step 5: Commit**

```bash
git add src/VStartNext.Core/Launch src/VStartNext.Infrastructure/Launch tests/VStartNext.Core.Tests/Launch/FlowLaunchRunnerTests.cs
git commit -m "feat: add ordered flow launch execution pipeline"
```

### Task 6: Context Actions for Launch Cards

**Files:**
- Create: `src/VStartNext.Core/Launch/LaunchContextAction.cs`
- Create: `src/VStartNext.Core/Launch/LaunchContextService.cs`
- Test: `tests/VStartNext.Core.Tests/Launch/LaunchContextServiceTests.cs`

**Step 1: Write the failing test**

```csharp
[Fact]
public void GetActions_ProvidesRunAsAdminOpenDirCopyPath()
{
    var service = new LaunchContextService();
    var actions = service.GetActions("C:\\Tools\\app.exe").Select(x => x.Type);
    actions.Should().Contain([RunAsAdmin, OpenDirectory, CopyPath]);
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter GetActions_ProvidesRunAsAdminOpenDirCopyPath -v minimal`  
Expected: FAIL with missing context service.

**Step 3: Write minimal implementation**

Return three baseline actions with command metadata.

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter GetActions_ProvidesRunAsAdminOpenDirCopyPath -v minimal`  
Expected: PASS.

**Step 5: Commit**

```bash
git add src/VStartNext.Core/Launch tests/VStartNext.Core.Tests/Launch/LaunchContextServiceTests.cs
git commit -m "feat: add launch card context actions"
```

### Task 7: Theme Engine v1 Token Set

**Files:**
- Create: `src/VStartNext.App/Styles/NeoThemeTokens.cs`
- Modify: `src/VStartNext.App/Styles/ThemeResources.xaml`
- Test: `tests/VStartNext.Core.Tests/UI/NeoThemeTokensTests.cs`

**Step 1: Write the failing test**

```csharp
[Fact]
public void DefaultTheme_HasRequiredTokens()
{
    var tokens = NeoThemeTokens.Default();
    tokens.RadiusLarge.Should().Be(16);
    tokens.SpacingLg.Should().Be(24);
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter DefaultTheme_HasRequiredTokens -v minimal`  
Expected: FAIL with missing token class.

**Step 3: Write minimal implementation**

Add typed token model + defaults; map to resource dictionary keys.

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter DefaultTheme_HasRequiredTokens -v minimal`  
Expected: PASS.

**Step 5: Commit**

```bash
git add src/VStartNext.App/Styles tests/VStartNext.Core.Tests/UI/NeoThemeTokensTests.cs
git commit -m "feat: add theme engine v1 token baseline"
```

### Task 8: Command Bar + Grid + Side Panel Composition

**Files:**
- Modify: `src/VStartNext.App/Windows/NeoPanelView.cs`
- Modify: `src/VStartNext.App/Windows/ShellWindowForm.cs`
- Create: `src/VStartNext.App/Windows/Controls/CommandBarControl.cs`
- Create: `src/VStartNext.App/Windows/Controls/LaunchGridControl.cs`
- Create: `src/VStartNext.App/Windows/Controls/ContextPanelControl.cs`
- Test: `tests/VStartNext.Core.Tests/UI/NeoPanelCompositionTests.cs`

**Step 1: Write the failing test**

```csharp
[Fact]
public void NeoPanel_ContainsCommandBarGridAndContextPanel()
{
    var view = new NeoPanelView();
    view.HasCommandBar.Should().BeTrue();
    view.HasLaunchGrid.Should().BeTrue();
    view.HasContextPanel.Should().BeTrue();
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter NeoPanel_ContainsCommandBarGridAndContextPanel -v minimal`  
Expected: FAIL with missing flags/controls.

**Step 3: Write minimal implementation**

Compose three controls in panel zones and expose testable flags.

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter NeoPanel_ContainsCommandBarGridAndContextPanel -v minimal`  
Expected: PASS.

**Step 5: Commit**

```bash
git add src/VStartNext.App/Windows tests/VStartNext.Core.Tests/UI/NeoPanelCompositionTests.cs
git commit -m "feat: compose neo-panel command bar grid and context panel"
```

### Task 9: End-to-End Shell Interaction Wiring

**Files:**
- Modify: `src/VStartNext.App/App.xaml.cs`
- Modify: `src/VStartNext.App/Program.cs`
- Test: `tests/VStartNext.Core.Tests/UI/NeoPanelInteractionFlowTests.cs`

**Step 1: Write the failing test**

```csharp
[Fact]
public async Task CommandBarInput_RoutesToPaletteAndUpdatesResultState()
{
    var app = new App(enableSystemTrayIcon: false);
    var updated = await app.HandleCommandInputAsync("calc: 2*36");
    updated.DisplayText.Should().Be("72");
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter CommandBarInput_RoutesToPaletteAndUpdatesResultState -v minimal`  
Expected: FAIL with missing app command route.

**Step 3: Write minimal implementation**

Wire command bar input to command palette service and expose state update.

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter CommandBarInput_RoutesToPaletteAndUpdatesResultState -v minimal`  
Expected: PASS.

**Step 5: Commit**

```bash
git add src/VStartNext.App tests/VStartNext.Core.Tests/UI/NeoPanelInteractionFlowTests.cs
git commit -m "feat: wire command bar interaction through app orchestration"
```

### Task 10: Verification and Documentation Refresh

**Files:**
- Modify: `scripts/verify.ps1`
- Modify: `docs/qa/mvp-checklist.md`
- Modify: `README.md`

**Step 1: Write the failing test**

```powershell
# verify should fail if any neo-panel tests fail
dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter NeoPanel -v minimal
if ($LASTEXITCODE -ne 0) { throw "Neo-panel tests failed" }
```

**Step 2: Run test to verify it fails**

Run: `powershell -ExecutionPolicy Bypass -File scripts/verify.ps1`  
Expected: FAIL until all neo-panel tasks are implemented.

**Step 3: Write minimal implementation**

Update verify script and QA checklist to include:

1. command prefix behavior
2. flow launch behavior
3. visibility interaction behavior

**Step 4: Run test to verify it passes**

Run: `powershell -ExecutionPolicy Bypass -File scripts/verify.ps1`  
Expected: PASS and `Verification passed`.

**Step 5: Commit**

```bash
git add scripts/verify.ps1 docs/qa/mvp-checklist.md README.md
git commit -m "chore: refresh verification and docs for neo-panel wave"
```

## Final Verification Gate

Run:

```bash
dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj -v minimal
dotnet build VStartNext.sln -c Release
powershell -ExecutionPolicy Bypass -File scripts/verify.ps1
```

Expected:

1. All tests pass
2. Release build succeeds
3. Verification script exits with success
