# VStart Next AI Agent (Wave 3) Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Build a cloud-first, bilingual, safety-guarded launcher agent MVP that can turn natural language into executable multi-step tool plans.

**Architecture:** Add a new `Agent` module in `VStartNext.Core` for contracts, planner interfaces, policy, execution, orchestration, and memory. Wire the launcher app command path to route AI intents through orchestrator while preserving the existing command palette path as fallback.

**Tech Stack:** .NET 8, C#, existing Windows Forms shell host, VStartNext.Core/VStartNext.App/VStartNext.Infrastructure, xUnit, FluentAssertions

---

## Plan Rules

1. Follow @test-driven-development for each behavior change.
2. Use @verification-before-completion before completion claims.
3. Keep one atomic commit per task.

### Task 1: Agent Domain Contracts

**Files:**
- Create: `src/VStartNext.Core/Agent/AgentIntent.cs`
- Create: `src/VStartNext.Core/Agent/AgentRiskLevel.cs`
- Create: `src/VStartNext.Core/Agent/AgentPlanStep.cs`
- Create: `src/VStartNext.Core/Agent/AgentActionPlan.cs`
- Create: `src/VStartNext.Core/Agent/AgentRunResult.cs`
- Test: `tests/VStartNext.Core.Tests/Agent/AgentContractsTests.cs`

**Step 1: Write the failing test**

```csharp
[Fact]
public void AgentPlan_CanRepresentMultiStepBilingualIntent()
{
    var plan = new AgentActionPlan(
        AgentIntent.Automation,
        "打开 Chrome and open github",
        [new AgentPlanStep("launch_app", "chrome", AgentRiskLevel.Low)]);

    plan.Steps.Should().HaveCount(1);
    plan.Intent.Should().Be(AgentIntent.Automation);
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter AgentPlan_CanRepresentMultiStepBilingualIntent -v minimal`  
Expected: FAIL with missing agent contract types.

**Step 3: Write minimal implementation**

```csharp
public enum AgentIntent { Search, Automation }
public enum AgentRiskLevel { Low, Medium, High }
public sealed record AgentPlanStep(string ToolName, string Arguments, AgentRiskLevel RiskLevel);
public sealed record AgentActionPlan(AgentIntent Intent, string OriginalInput, IReadOnlyList<AgentPlanStep> Steps);
```

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter AgentPlan_CanRepresentMultiStepBilingualIntent -v minimal`  
Expected: PASS.

**Step 5: Commit**

```bash
git add src/VStartNext.Core/Agent tests/VStartNext.Core.Tests/Agent/AgentContractsTests.cs
git commit -m "feat: add agent core domain contracts"
```

### Task 2: Planner and Model Router Contracts

**Files:**
- Create: `src/VStartNext.Core/Agent/IAgentPlanner.cs`
- Create: `src/VStartNext.Core/Agent/IAgentModelRouter.cs`
- Create: `src/VStartNext.Core/Agent/AgentPlannerRequest.cs`
- Create: `src/VStartNext.Core/Agent/AgentLanguage.cs`
- Test: `tests/VStartNext.Core.Tests/Agent/AgentPlannerContractsTests.cs`

**Step 1: Write the failing test**

```csharp
[Fact]
public async Task PlannerRequest_CarriesInputLanguageAndToolCatalog()
{
    var request = new AgentPlannerRequest("open chrome", AgentLanguage.English, ["launch_app"]);
    request.Language.Should().Be(AgentLanguage.English);
    request.AvailableTools.Should().Contain("launch_app");
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter PlannerRequest_CarriesInputLanguageAndToolCatalog -v minimal`  
Expected: FAIL with missing planner contract types.

**Step 3: Write minimal implementation**

```csharp
public enum AgentLanguage { Chinese, English, Mixed }
public sealed record AgentPlannerRequest(string Input, AgentLanguage Language, IReadOnlyList<string> AvailableTools);
public interface IAgentPlanner { Task<AgentActionPlan> PlanAsync(AgentPlannerRequest request); }
public interface IAgentModelRouter { Task<string> CompleteAsync(string prompt); }
```

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter PlannerRequest_CarriesInputLanguageAndToolCatalog -v minimal`  
Expected: PASS.

**Step 5: Commit**

```bash
git add src/VStartNext.Core/Agent tests/VStartNext.Core.Tests/Agent/AgentPlannerContractsTests.cs
git commit -m "feat: add agent planner and model router contracts"
```

### Task 3: Tool Registry and Runtime

**Files:**
- Create: `src/VStartNext.Core/Agent/IAgentTool.cs`
- Create: `src/VStartNext.Core/Agent/AgentToolResult.cs`
- Create: `src/VStartNext.Core/Agent/AgentToolRegistry.cs`
- Test: `tests/VStartNext.Core.Tests/Agent/AgentToolRegistryTests.cs`

**Step 1: Write the failing test**

```csharp
[Fact]
public async Task Registry_ResolvesAndExecutesRegisteredTool()
{
    var registry = new AgentToolRegistry([new FakeTool("open_url")]);
    var result = await registry.ExecuteAsync("open_url", "https://openai.com");
    result.Success.Should().BeTrue();
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter Registry_ResolvesAndExecutesRegisteredTool -v minimal`  
Expected: FAIL with missing registry/runtime classes.

**Step 3: Write minimal implementation**

Implement:

1. Tool interface (`Name`, `ExecuteAsync`)
2. Registry dictionary by tool name
3. `ExecuteAsync` returns failure for unknown tool

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter Registry_ResolvesAndExecutesRegisteredTool -v minimal`  
Expected: PASS.

**Step 5: Commit**

```bash
git add src/VStartNext.Core/Agent tests/VStartNext.Core.Tests/Agent/AgentToolRegistryTests.cs
git commit -m "feat: add agent tool registry and runtime"
```

### Task 4: Policy Guard for High-Risk Confirmation

**Files:**
- Create: `src/VStartNext.Core/Agent/AgentPolicyDecision.cs`
- Create: `src/VStartNext.Core/Agent/AgentPolicyGuard.cs`
- Test: `tests/VStartNext.Core.Tests/Agent/AgentPolicyGuardTests.cs`

**Step 1: Write the failing test**

```csharp
[Fact]
public void Evaluate_HighRiskStep_RequiresConfirmation()
{
    var guard = new AgentPolicyGuard();
    var decision = guard.Evaluate(new AgentPlanStep("quick_action", "shutdown", AgentRiskLevel.High));
    decision.RequiresUserConfirmation.Should().BeTrue();
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter Evaluate_HighRiskStep_RequiresConfirmation -v minimal`  
Expected: FAIL with missing policy guard.

**Step 3: Write minimal implementation**

Implement:

1. `Evaluate` method
2. Rule: `High` risk always requires confirmation
3. Medium/Low return allowed by default

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter Evaluate_HighRiskStep_RequiresConfirmation -v minimal`  
Expected: PASS.

**Step 5: Commit**

```bash
git add src/VStartNext.Core/Agent tests/VStartNext.Core.Tests/Agent/AgentPolicyGuardTests.cs
git commit -m "feat: add agent policy guard for high-risk confirmation"
```

### Task 5: Agent Executor with Step-by-Step Outcomes

**Files:**
- Create: `src/VStartNext.Core/Agent/AgentStepExecution.cs`
- Create: `src/VStartNext.Core/Agent/AgentExecutor.cs`
- Test: `tests/VStartNext.Core.Tests/Agent/AgentExecutorTests.cs`

**Step 1: Write the failing test**

```csharp
[Fact]
public async Task ExecuteAsync_RunsAllStepsInOrder()
{
    var executor = new AgentExecutor(registry, new AgentPolicyGuard());
    var result = await executor.ExecuteAsync(plan, autoConfirmHighRisk: true);
    result.Executions.Select(x => x.ToolName).Should().Equal("launch_app", "open_url");
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter ExecuteAsync_RunsAllStepsInOrder -v minimal`  
Expected: FAIL with missing executor.

**Step 3: Write minimal implementation**

Implement:

1. Sequential iteration over steps
2. Policy check per step
3. Tool execution and output collection
4. Stop on failure and return aggregated result

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter ExecuteAsync_RunsAllStepsInOrder -v minimal`  
Expected: PASS.

**Step 5: Commit**

```bash
git add src/VStartNext.Core/Agent tests/VStartNext.Core.Tests/Agent/AgentExecutorTests.cs
git commit -m "feat: add agent executor with ordered step outcomes"
```

### Task 6: Orchestrator with Reflection Hook

**Files:**
- Create: `src/VStartNext.Core/Agent/IAgentReflectionService.cs`
- Create: `src/VStartNext.Core/Agent/AgentOrchestrator.cs`
- Test: `tests/VStartNext.Core.Tests/Agent/AgentOrchestratorTests.cs`

**Step 1: Write the failing test**

```csharp
[Fact]
public async Task RunAsync_InvokesReflectionBeforeExecution()
{
    var reflection = new FakeReflectionService();
    var orchestrator = new AgentOrchestrator(planner, executor, reflection, memory);
    await orchestrator.RunAsync("打开 chrome");
    reflection.Called.Should().BeTrue();
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter RunAsync_InvokesReflectionBeforeExecution -v minimal`  
Expected: FAIL with missing orchestrator/reflection types.

**Step 3: Write minimal implementation**

Implement:

1. Planner call
2. Reflection call on plan
3. Executor call
4. Return final `AgentRunResult`

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter RunAsync_InvokesReflectionBeforeExecution -v minimal`  
Expected: PASS.

**Step 5: Commit**

```bash
git add src/VStartNext.Core/Agent tests/VStartNext.Core.Tests/Agent/AgentOrchestratorTests.cs
git commit -m "feat: add agent orchestrator with reflection hook"
```

### Task 7: Bilingual Response Policy

**Files:**
- Create: `src/VStartNext.Core/Agent/AgentResponseLanguagePolicy.cs`
- Test: `tests/VStartNext.Core.Tests/Agent/AgentResponseLanguagePolicyTests.cs`

**Step 1: Write the failing test**

```csharp
[Fact]
public void Resolve_FollowsInputLanguageByDefault()
{
    var policy = new AgentResponseLanguagePolicy();
    var result = policy.Resolve("open chrome", uiLanguage: "zh-CN", followUiLanguage: false);
    result.Should().Be(AgentLanguage.English);
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter Resolve_FollowsInputLanguageByDefault -v minimal`  
Expected: FAIL with missing response language policy.

**Step 3: Write minimal implementation**

Implement:

1. Input language detection (Chinese/English/Mixed heuristic)
2. Toggle to force UI language
3. Default follows input language

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter Resolve_FollowsInputLanguageByDefault -v minimal`  
Expected: PASS.

**Step 5: Commit**

```bash
git add src/VStartNext.Core/Agent tests/VStartNext.Core.Tests/Agent/AgentResponseLanguagePolicyTests.cs
git commit -m "feat: add bilingual response language policy"
```

### Task 8: App Integration for Agent Routing

**Files:**
- Modify: `src/VStartNext.App/App.xaml.cs`
- Create: `src/VStartNext.App/Agent/AppAgentGateway.cs`
- Test: `tests/VStartNext.Core.Tests/UI/AppAgentRoutingTests.cs`

**Step 1: Write the failing test**

```csharp
[Fact]
public async Task HandleCommandInputAsync_NaturalLanguage_RoutesToAgent()
{
    var app = new App(enableSystemTrayIcon: false, agentGateway: new FakeAgentGateway("done"));
    var result = await app.HandleCommandInputAsync("打开 chrome 并打开 github");
    result.DisplayText.Should().Be("done");
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter HandleCommandInputAsync_NaturalLanguage_RoutesToAgent -v minimal`  
Expected: FAIL with missing app-level agent routing.

**Step 3: Write minimal implementation**

Implement:

1. Add `IAppAgentGateway` abstraction
2. In `App.HandleCommandInputAsync`, route NL input to agent gateway
3. Keep existing `calc/url/ws` palette path unchanged

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter HandleCommandInputAsync_NaturalLanguage_RoutesToAgent -v minimal`  
Expected: PASS.

**Step 5: Commit**

```bash
git add src/VStartNext.App tests/VStartNext.Core.Tests/UI/AppAgentRoutingTests.cs
git commit -m "feat: route natural-language commands through app agent gateway"
```

### Task 9: Memory Profile for User Habits

**Files:**
- Create: `src/VStartNext.Core/Agent/AgentMemoryProfile.cs`
- Create: `src/VStartNext.Core/Agent/AgentMemoryStore.cs`
- Test: `tests/VStartNext.Core.Tests/Agent/AgentMemoryStoreTests.cs`

**Step 1: Write the failing test**

```csharp
[Fact]
public void RecordRun_UpdatesPreferredLanguageAndToolFrequency()
{
    var store = new AgentMemoryStore();
    store.RecordRun(AgentLanguage.Chinese, "launch_app");
    store.Profile.PreferredLanguage.Should().Be(AgentLanguage.Chinese);
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter RecordRun_UpdatesPreferredLanguageAndToolFrequency -v minimal`  
Expected: FAIL with missing memory store/profile.

**Step 3: Write minimal implementation**

Implement:

1. In-memory profile with language + tool frequency
2. `RecordRun` update behavior
3. Read-only snapshot accessor

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter RecordRun_UpdatesPreferredLanguageAndToolFrequency -v minimal`  
Expected: PASS.

**Step 5: Commit**

```bash
git add src/VStartNext.Core/Agent tests/VStartNext.Core.Tests/Agent/AgentMemoryStoreTests.cs
git commit -m "feat: add agent memory profile for habit learning"
```

### Task 10: Verification and Documentation Refresh for Wave 3

**Files:**
- Modify: `scripts/verify.ps1`
- Modify: `docs/qa/mvp-checklist.md`
- Modify: `README.md`

**Step 1: Write the failing test**

```powershell
# verify should fail if agent tests fail
dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj --filter Agent -v minimal
if ($LASTEXITCODE -ne 0) { throw "Agent tests failed" }
```

**Step 2: Run test to verify it fails**

Run: `powershell -ExecutionPolicy Bypass -File scripts/verify.ps1`  
Expected: FAIL until all agent tasks are implemented.

**Step 3: Write minimal implementation**

Update verification and docs to include:

1. agent orchestration behavior
2. bilingual response policy checks
3. high-risk confirmation policy
4. app-level agent routing checks

**Step 4: Run test to verify it passes**

Run: `powershell -ExecutionPolicy Bypass -File scripts/verify.ps1`  
Expected: PASS and `Verification passed`.

**Step 5: Commit**

```bash
git add scripts/verify.ps1 docs/qa/mvp-checklist.md README.md
git commit -m "chore: refresh verification and docs for wave 3 ai-agent"
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

