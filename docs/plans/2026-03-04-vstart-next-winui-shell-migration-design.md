# VStart Next WinUI Shell Migration Design

Date: 2026-03-04  
Status: Approved (owner-directed, experience-based default)  
Platform: Windows only

## 1. Goal

Build a modern WinUI-first shell path without breaking current delivery speed.  
The migration must preserve current Core/Infrastructure behavior and keep release confidence high.

## 2. Constraints

1. Existing app is Windows Forms entrypoint and fully tested.
2. AI execution, audit, model settings, and safety flows are already integrated into that shell path.
3. We need visible UI modernization progress while keeping fast iteration and low regression risk.

## 3. Approaches

### Approach A: Big-bang rewrite to WinUI

Replace Windows Forms shell and dialogs in one wave.

Pros:
1. Fastest path to pure WinUI architecture.
2. No temporary adapters.

Cons:
1. Highest regression risk for hotkey/tray/agent flows.
2. Large unstable PRs and longer freeze windows.
3. Harder rollback.

### Approach B: Dual-track shell migration (Recommended)

Introduce a shell-host abstraction and a host-mode switch.  
Keep WinForms as stable default while adding WinUI host incrementally.

Pros:
1. Low-risk, testable incremental migration.
2. Easy fallback to WinForms.
3. Keeps current feature velocity.

Cons:
1. Temporary duplication in shell layer.
2. Slightly longer total migration duration than big-bang.

### Approach C: Keep WinForms shell, only visual restyle

Modernize visuals but keep WinForms as long-term shell.

Pros:
1. Lowest immediate cost.
2. Fast cosmetic wins.

Cons:
1. Misses WinUI architecture target.
2. Harder to reach long-term Fluent fidelity and future platform features.

## 4. Selected Design

Choose **Approach B (Dual-track migration)**.

### 4.1 Architecture

1. Add a shell host contract (`IAppShellHost`) used by `Program`.
2. Move shell creation into `ShellHostFactory` with `ShellHostMode` (`WinForms`, `WinUIPreview`).
3. Keep `WinFormsShellHost` as default and current production path.
4. Add `WinUiPreviewShellHost` placeholder/stub first, then replace with real WinUI implementation in follow-up waves.

### 4.2 Data/Control Flow

1. `Program` builds agent/app services once.
2. `Program` resolves host mode and creates `IAppShellHost`.
3. `Program` subscribes to `CommandSubmitted` from host contract, not concrete form.
4. Visibility control continues through `ShellWindowController` and `IShellWindow`.
5. Agent dialogs and model settings keep owner-window semantics via host contract.

### 4.3 Error Handling and Fallback

1. Unknown or unsupported host mode falls back to WinForms.
2. WinUI preview host failures must not block app startup; fallback path logs and uses WinForms.
3. Keep existing high-risk confirmation and cancellation behavior unchanged.

### 4.4 Testing

1. Unit tests for `ShellHostFactory` mode resolution and fallback.
2. Unit tests for default host selection behavior.
3. Regression tests for existing shell interaction path remain green.

## 5. Incremental Delivery Plan

1. Wave 1: Host abstraction + factory + mode routing (no UI behavior change).
2. Wave 2: WinUI preview shell with command bar and neo-panel scaffold.
3. Wave 3: Migrate settings and agent progress dialogs to WinUI surfaces.
4. Wave 4: Switch default mode to WinUI after QA gates and soak run.
