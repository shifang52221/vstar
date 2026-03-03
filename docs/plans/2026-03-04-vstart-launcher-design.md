# VStart Next (Windows) Design

Date: 2026-03-04
Status: Approved
Platform: Windows only
Tech direction: WinUI 3 (C#), MVVM

## 1. Goals

Build a modern Windows launcher that clearly outperforms legacy VStart-style tools in:

1. Interaction speed (open, search, launch)
2. Visual quality (modern Fluent-based UI)
3. Reliability (stable long-running tray app)

## 2. MVP Scope

The first release includes only high-frequency core workflows:

1. Global hotkey: `Alt+Space` to show/hide launcher.
2. Instant search: apps, configured folders, URLs.
3. Group panel: category-based shortcuts with drag-sort and pin-favorites.
4. System actions: shutdown, restart, lock screen, open settings.
5. Modern UI: Fluent style, lightweight motion, acrylic/mica feel.
6. Local-only persistence: JSON config, no cloud sync in MVP.

Out of scope for MVP:

1. Plugin marketplace
2. Cloud sync and account system
3. Cross-platform support

## 3. Architecture

Use a modular WinUI 3 + MVVM structure:

1. `Shell` module
   - Main window lifecycle
   - Tray icon/menu
   - Global hotkey
   - Startup registration
2. `Search` module
   - Query pipeline
   - Ranking
   - Async execution and cancellation
3. `Launch` module
   - App/file/url/system action execution
   - Execution result reporting
4. `Catalog` module
   - Groups, pins, ordering
   - Drag-and-drop state updates
5. `Theme` module
   - Fluent visual tokens and motion settings
   - Theme presets and style variables
6. `Storage` module
   - JSON read/write
   - Schema versioning and migration

## 4. Data Flow and Performance Plan

### 4.1 Startup and Show/Hide

1. App starts as tray resident process.
2. Main panel is lightweight on first paint.
3. Hotkey toggles visibility and focus without heavy synchronous work.

### 4.2 Search Pipeline

1. Input debounce at 50ms.
2. Every new query cancels previous in-flight task.
3. Return hot-cache results first, then refresh with index results.
4. Prevent out-of-order responses from overwriting newer results.

### 4.3 Ranking

Composite score:

1. Historical usage frequency
2. Recency of use
3. Group pin/priority weight

### 4.4 Index Strategy

1. Background low-priority indexing.
2. Incremental updates on app/config changes.
3. User-configured folder ranges for file lookup.

### 4.5 MVP Performance Targets

1. Cold start: `<1.5s`
2. Hotkey to interactive panel: `<120ms`
3. Query to visible result update: `<80ms` (typical)

## 5. Error Handling and Resilience

1. Fault isolation:
   - Search/index/theme failures must not crash shell/tray lifecycle.
2. Action error clarity:
   - Explicit messages for missing target, permission issues, invalid path.
3. Config self-healing:
   - Backup corrupted JSON, recover with defaults, preserve salvageable fields.
4. Safe execution boundary:
   - System/custom commands pass allowlist validation.
5. Graceful degradation:
   - If index fails, fallback to baseline lookup with reduced ranking quality.

## 6. Testing Strategy

1. Unit tests
   - Ranking score logic
   - Query matching
   - Config migration
2. Integration tests
   - Hotkey -> search -> launch full flow
3. Performance regression
   - Fixed corpus, track P50/P95 query latency
4. Soak and stability
   - 24h resident run
   - Monitor memory growth and handle leaks

## 7. Release Strategy

1. Internal beta (10-30 users) first.
2. Prioritize crash fixes and false-launch issues.
3. Two-week iterations, small releases, rollback package retained.

## 8. Non-Functional Requirements (Best-Practice Baseline)

1. UI thread safety:
   - Keep search and indexing off UI thread.
2. Cancellation correctness:
   - Always cancel stale async work and ignore stale results.
3. Observability:
   - Track hotkey latency, query latency, launch success rate.
4. Backward compatibility:
   - Storage schema versioning with explicit migration path.

## 9. Future Extensions (Post-MVP)

1. Theme engine with deep skinning packs
2. Plugin SDK for commands and providers
3. Optional sync profile (settings/history)
