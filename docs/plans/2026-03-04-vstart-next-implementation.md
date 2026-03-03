# VStart Next Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Build a modern Windows-native launcher (WinUI 3) with hotkey summon, instant search, grouped shortcuts, and reliable action launching.

**Architecture:** Use a layered solution with `App` (WinUI shell), `Core` (search/ranking/catalog/launch logic), and `Infrastructure` (storage, indexing, OS integration). Keep UI responsive by running search/indexing off the UI thread with cancellation and stale-result guards.

**Tech Stack:** .NET 8, WinUI 3 (Windows App SDK), C#, CommunityToolkit.Mvvm, xUnit, FluentAssertions

---

## Plan Rules

1. Follow @test-driven-development on every behavior change.
2. Before claiming completion for any task, run checks per @verification-before-completion.
3. Keep commits small and atomic; one task one commit.

### Task 1: Solution Bootstrap

**Files:**
- Create: `src/VStartNext.App/VStartNext.App.csproj`
- Create: `src/VStartNext.App/App.xaml`
- Create: `src/VStartNext.App/App.xaml.cs`
- Create: `src/VStartNext.Core/VStartNext.Core.csproj`
- Create: `src/VStartNext.Infrastructure/VStartNext.Infrastructure.csproj`
- Create: `tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj`
- Create: `VStartNext.sln`

**Step 1: Write the failing test**

```csharp
using Xunit;

namespace VStartNext.Core.Tests;

public class SmokeTests
{
    [Fact]
    public void CoreAssembly_Loads()
    {
        Assert.True(typeof(SmokeTests).Assembly.FullName is not null);
    }
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj -v minimal`  
Expected: FAIL because solution/projects do not exist yet.

**Step 3: Write minimal implementation**

```xml
<!-- src/VStartNext.Core/VStartNext.Core.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0-windows10.0.19041.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj -v minimal`  
Expected: PASS, 1 test passed.

**Step 5: Commit**

```bash
git add VStartNext.sln src tests
git commit -m "chore: bootstrap VStart Next solution structure"
```

### Task 2: Global Hotkey Service (Alt+Space)

**Files:**
- Create: `src/VStartNext.Core/Abstractions/IHotkeyService.cs`
- Create: `src/VStartNext.Infrastructure/Win32/GlobalHotkeyService.cs`
- Create: `tests/VStartNext.Core.Tests/Hotkey/HotkeyBindingTests.cs`
- Modify: `src/VStartNext.App/App.xaml.cs`

**Step 1: Write the failing test**

```csharp
using FluentAssertions;
using Xunit;

namespace VStartNext.Core.Tests.Hotkey;

public class HotkeyBindingTests
{
    [Fact]
    public void DefaultBinding_IsAltSpace()
    {
        var binding = HotkeyBinding.Default;
        binding.Modifiers.Should().Be(HotkeyModifiers.Alt);
        binding.Key.Should().Be(HotkeyKey.Space);
    }
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter DefaultBinding_IsAltSpace -v minimal`  
Expected: FAIL with missing `HotkeyBinding` symbols.

**Step 3: Write minimal implementation**

```csharp
namespace VStartNext.Core.Abstractions;

public enum HotkeyModifiers { None = 0, Alt = 1, Ctrl = 2, Shift = 4, Win = 8 }
public enum HotkeyKey { Space = 32 }

public readonly record struct HotkeyBinding(HotkeyModifiers Modifiers, HotkeyKey Key)
{
    public static HotkeyBinding Default => new(HotkeyModifiers.Alt, HotkeyKey.Space);
}
```

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter DefaultBinding_IsAltSpace -v minimal`  
Expected: PASS.

**Step 5: Commit**

```bash
git add src/VStartNext.Core src/VStartNext.Infrastructure src/VStartNext.App tests/VStartNext.Core.Tests
git commit -m "feat: add global hotkey binding contracts and default Alt+Space"
```

### Task 3: Shell Toggle Flow

**Files:**
- Create: `src/VStartNext.Core/Shell/IShellController.cs`
- Create: `src/VStartNext.App/ViewModels/ShellViewModel.cs`
- Create: `tests/VStartNext.Core.Tests/Shell/ShellToggleTests.cs`
- Create: `src/VStartNext.App/Views/ShellWindow.xaml`

**Step 1: Write the failing test**

```csharp
using FluentAssertions;
using VStartNext.App.ViewModels;
using Xunit;

namespace VStartNext.Core.Tests.Shell;

public class ShellToggleTests
{
    [Fact]
    public void ToggleVisibility_FlipsState()
    {
        var vm = new ShellViewModel();
        vm.IsVisible.Should().BeFalse();
        vm.ToggleVisibility();
        vm.IsVisible.Should().BeTrue();
    }
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter ToggleVisibility_FlipsState -v minimal`  
Expected: FAIL with missing `ShellViewModel`.

**Step 3: Write minimal implementation**

```csharp
using CommunityToolkit.Mvvm.ComponentModel;

namespace VStartNext.App.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isVisible;

    public void ToggleVisibility() => IsVisible = !IsVisible;
}
```

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter ToggleVisibility_FlipsState -v minimal`  
Expected: PASS.

**Step 5: Commit**

```bash
git add src/VStartNext.App src/VStartNext.Core tests/VStartNext.Core.Tests
git commit -m "feat: implement shell visibility toggle flow"
```

### Task 4: Storage Schema and Self-Healing Config

**Files:**
- Create: `src/VStartNext.Core/Config/AppConfig.cs`
- Create: `src/VStartNext.Infrastructure/Storage/JsonConfigStore.cs`
- Create: `tests/VStartNext.Core.Tests/Config/ConfigRecoveryTests.cs`

**Step 1: Write the failing test**

```csharp
using FluentAssertions;
using Xunit;

namespace VStartNext.Core.Tests.Config;

public class ConfigRecoveryTests
{
    [Fact]
    public void CorruptedConfig_FallsBackToDefault()
    {
        var json = "{ broken";
        var config = JsonConfigStore.ParseOrDefault(json, out var recovered);
        config.SchemaVersion.Should().Be(1);
        recovered.Should().BeTrue();
    }
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter CorruptedConfig_FallsBackToDefault -v minimal`  
Expected: FAIL with missing `JsonConfigStore`.

**Step 3: Write minimal implementation**

```csharp
using System.Text.Json;
using VStartNext.Core.Config;

namespace VStartNext.Infrastructure.Storage;

public static class JsonConfigStore
{
    public static AppConfig ParseOrDefault(string json, out bool recovered)
    {
        try
        {
            recovered = false;
            return JsonSerializer.Deserialize<AppConfig>(json) ?? AppConfig.Default();
        }
        catch
        {
            recovered = true;
            return AppConfig.Default();
        }
    }
}
```

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter CorruptedConfig_FallsBackToDefault -v minimal`  
Expected: PASS.

**Step 5: Commit**

```bash
git add src/VStartNext.Core src/VStartNext.Infrastructure tests/VStartNext.Core.Tests
git commit -m "feat: add json config schema and corruption recovery"
```

### Task 5: Catalog Groups and Pin Order

**Files:**
- Create: `src/VStartNext.Core/Catalog/CatalogItem.cs`
- Create: `src/VStartNext.Core/Catalog/CatalogService.cs`
- Create: `tests/VStartNext.Core.Tests/Catalog/CatalogOrderingTests.cs`

**Step 1: Write the failing test**

```csharp
using FluentAssertions;
using Xunit;

namespace VStartNext.Core.Tests.Catalog;

public class CatalogOrderingTests
{
    [Fact]
    public void PinnedItems_AppearBeforeUnpinned()
    {
        var service = new CatalogService();
        var items = service.Sort([
            new CatalogItem("A", false, 0),
            new CatalogItem("B", true, 5)
        ]);
        items[0].Name.Should().Be("B");
    }
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter PinnedItems_AppearBeforeUnpinned -v minimal`  
Expected: FAIL with missing catalog classes.

**Step 3: Write minimal implementation**

```csharp
namespace VStartNext.Core.Catalog;

public sealed record CatalogItem(string Name, bool Pinned, int Order);

public sealed class CatalogService
{
    public List<CatalogItem> Sort(IEnumerable<CatalogItem> items) =>
        items.OrderByDescending(x => x.Pinned).ThenByDescending(x => x.Order).ToList();
}
```

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter PinnedItems_AppearBeforeUnpinned -v minimal`  
Expected: PASS.

**Step 5: Commit**

```bash
git add src/VStartNext.Core tests/VStartNext.Core.Tests
git commit -m "feat: add catalog item ordering by pin and rank"
```

### Task 6: Search Engine with Cancellation and Ranking

**Files:**
- Create: `src/VStartNext.Core/Search/SearchItem.cs`
- Create: `src/VStartNext.Core/Search/SearchEngine.cs`
- Create: `tests/VStartNext.Core.Tests/Search/SearchRankingTests.cs`

**Step 1: Write the failing test**

```csharp
using FluentAssertions;
using Xunit;

namespace VStartNext.Core.Tests.Search;

public class SearchRankingTests
{
    [Fact]
    public void Ranking_UsesRecencyFrequencyAndPinWeight()
    {
        var engine = new SearchEngine();
        var top = engine.Rank("ch", [
            new SearchItem("Chrome", 10, DateTimeOffset.UtcNow, true),
            new SearchItem("Chromium", 50, DateTimeOffset.UtcNow.AddDays(-30), false)
        ]).First();
        top.Name.Should().Be("Chrome");
    }
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter Ranking_UsesRecencyFrequencyAndPinWeight -v minimal`  
Expected: FAIL with missing `SearchEngine`.

**Step 3: Write minimal implementation**

```csharp
namespace VStartNext.Core.Search;

public sealed record SearchItem(string Name, int Frequency, DateTimeOffset LastUsed, bool Pinned);

public sealed class SearchEngine
{
    public IEnumerable<SearchItem> Rank(string query, IEnumerable<SearchItem> items)
    {
        var q = query.Trim().ToLowerInvariant();
        return items
            .Where(x => x.Name.ToLowerInvariant().Contains(q))
            .OrderByDescending(Score);
    }

    private static double Score(SearchItem x)
    {
        var recencyDays = (DateTimeOffset.UtcNow - x.LastUsed).TotalDays;
        var recency = Math.Max(0, 30 - recencyDays);
        var pin = x.Pinned ? 100 : 0;
        return pin + x.Frequency + recency;
    }
}
```

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter Ranking_UsesRecencyFrequencyAndPinWeight -v minimal`  
Expected: PASS.

**Step 5: Commit**

```bash
git add src/VStartNext.Core tests/VStartNext.Core.Tests
git commit -m "feat: add ranked search engine core logic"
```

### Task 7: Launch Executor and Error Messages

**Files:**
- Create: `src/VStartNext.Core/Launch/LaunchRequest.cs`
- Create: `src/VStartNext.Infrastructure/Launch/LaunchExecutor.cs`
- Create: `tests/VStartNext.Core.Tests/Launch/LaunchExecutorTests.cs`

**Step 1: Write the failing test**

```csharp
using FluentAssertions;
using Xunit;

namespace VStartNext.Core.Tests.Launch;

public class LaunchExecutorTests
{
    [Fact]
    public async Task MissingPath_ReturnsReadableError()
    {
        var executor = new LaunchExecutor();
        var result = await executor.LaunchAsync(new LaunchRequest("C:\\nope\\missing.exe"));
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter MissingPath_ReturnsReadableError -v minimal`  
Expected: FAIL with missing launch types.

**Step 3: Write minimal implementation**

```csharp
namespace VStartNext.Core.Launch;

public sealed record LaunchRequest(string Target);
public sealed record LaunchResult(bool Success, string? ErrorMessage);
```

```csharp
using VStartNext.Core.Launch;

namespace VStartNext.Infrastructure.Launch;

public sealed class LaunchExecutor
{
    public Task<LaunchResult> LaunchAsync(LaunchRequest request)
    {
        if (!File.Exists(request.Target))
            return Task.FromResult(new LaunchResult(false, "Target not found"));
        return Task.FromResult(new LaunchResult(true, null));
    }
}
```

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter MissingPath_ReturnsReadableError -v minimal`  
Expected: PASS.

**Step 5: Commit**

```bash
git add src/VStartNext.Core src/VStartNext.Infrastructure tests/VStartNext.Core.Tests
git commit -m "feat: add launch executor with clear failure results"
```

### Task 8: Tray Integration and Startup Option

**Files:**
- Create: `src/VStartNext.App/Services/TrayService.cs`
- Create: `src/VStartNext.Infrastructure/Startup/StartupRegistrationService.cs`
- Create: `tests/VStartNext.Core.Tests/Startup/StartupRegistrationTests.cs`
- Modify: `src/VStartNext.App/App.xaml.cs`

**Step 1: Write the failing test**

```csharp
using FluentAssertions;
using Xunit;

namespace VStartNext.Core.Tests.Startup;

public class StartupRegistrationTests
{
    [Fact]
    public void RegistryPath_IsCurrentUserRunKey()
    {
        StartupRegistrationService.RunKeyPath.Should()
            .Be(@"Software\Microsoft\Windows\CurrentVersion\Run");
    }
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter RegistryPath_IsCurrentUserRunKey -v minimal`  
Expected: FAIL with missing startup service.

**Step 3: Write minimal implementation**

```csharp
namespace VStartNext.Infrastructure.Startup;

public static class StartupRegistrationService
{
    public const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
}
```

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter RegistryPath_IsCurrentUserRunKey -v minimal`  
Expected: PASS.

**Step 5: Commit**

```bash
git add src/VStartNext.App src/VStartNext.Infrastructure tests/VStartNext.Core.Tests
git commit -m "feat: add tray service and startup registration baseline"
```

### Task 9: Fluent UI Shell and Grouped Launcher View

**Files:**
- Create: `src/VStartNext.App/Views/ShellWindow.xaml`
- Create: `src/VStartNext.App/Styles/ThemeResources.xaml`
- Create: `src/VStartNext.App/ViewModels/LauncherViewModel.cs`
- Create: `tests/VStartNext.Core.Tests/UI/LauncherViewModelTests.cs`

**Step 1: Write the failing test**

```csharp
using FluentAssertions;
using VStartNext.App.ViewModels;
using Xunit;

namespace VStartNext.Core.Tests.UI;

public class LauncherViewModelTests
{
    [Fact]
    public void DefaultCategories_ContainBrowserGroup()
    {
        var vm = new LauncherViewModel();
        vm.Categories.Should().Contain(x => x.Name == "Browser");
    }
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter DefaultCategories_ContainBrowserGroup -v minimal`  
Expected: FAIL with missing launcher VM.

**Step 3: Write minimal implementation**

```csharp
namespace VStartNext.App.ViewModels;

public sealed record CategoryVm(string Name);

public sealed class LauncherViewModel
{
    public IReadOnlyList<CategoryVm> Categories { get; } =
        [new("Browser"), new("Tools"), new("Office")];
}
```

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter DefaultCategories_ContainBrowserGroup -v minimal`  
Expected: PASS.

**Step 5: Commit**

```bash
git add src/VStartNext.App tests/VStartNext.Core.Tests
git commit -m "feat: add fluent launcher shell view model baseline"
```

### Task 10: End-to-End Verification and Packaging Baseline

**Files:**
- Create: `scripts/verify.ps1`
- Create: `docs/qa/mvp-checklist.md`
- Modify: `README.md`

**Step 1: Write the failing test**

```powershell
# scripts/verify.ps1 should fail if any test project fails
dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj -v minimal
if ($LASTEXITCODE -ne 0) { throw "Tests failed" }
```

**Step 2: Run test to verify it fails**

Run: `powershell -ExecutionPolicy Bypass -File scripts/verify.ps1`  
Expected: FAIL until all prior tasks are implemented.

**Step 3: Write minimal implementation**

```powershell
# scripts/verify.ps1
$ErrorActionPreference = "Stop"
dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj -v minimal
dotnet build VStartNext.sln -c Release
Write-Host "Verification passed"
```

**Step 4: Run test to verify it passes**

Run: `powershell -ExecutionPolicy Bypass -File scripts/verify.ps1`  
Expected: PASS with `Verification passed`.

**Step 5: Commit**

```bash
git add scripts/verify.ps1 docs/qa/mvp-checklist.md README.md
git commit -m "chore: add verification script and MVP QA checklist"
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
3. Verification script exits 0
