# VStart Next Neo-Panel Design (Wave 2)

Date: 2026-03-04
Status: Approved
Priority: Surpass legacy Vstart in both visual identity and functional depth

## 1. Objective

Deliver a launcher experience that is clearly ahead of old Vstart-style tools in:

1. First-look visual impact (modern, intentional, premium)
2. Daily productivity (fewer steps, faster decisions)
3. Functional differentiation (capabilities legacy launchers do not provide)

## 2. Strategy

Adopt a dual-track strategy:

1. Visual-first shell upgrade (`Neo-Panel`)
2. Frontier feature pack in the same wave

The product must avoid becoming only a skin refresh.

## 3. Neo-Panel Visual Architecture

The launcher shell uses five fixed zones:

1. Top: full-width command bar
2. Left: vertical navigation rail (`Home`, `Recent`, `Groups`, `Settings`)
3. Center: grouped launch grid
4. Right: context side panel (`Recent`, `Quick Actions`, runtime state)
5. Bottom: status strip (hotkey, index state, response latency)

## 4. Visual System Rules

Use centralized design tokens:

1. Radius tokens: `12`, `16`
2. Spacing tokens: `8`, `12`, `16`, `24`
3. Motion timing: `120-180ms`
4. Palette direction: dark-neutral base + high-contrast accent

Motion profile:

1. Shell open: fade + Y offset (`8px`)
2. Grid cards: staggered reveal
3. Reduced-motion fallback in settings

## 5. Interaction Components

### 5.1 CommandBar

1. Type-to-search, instant suggestions
2. `Enter` launches top match
3. `Ctrl+K` focuses command bar

### 5.2 LaunchGrid

1. Large card-based items
2. Group headers and collapse support
3. Arrow-key focus navigation
4. Enter/double-click launch

### 5.3 RecentRail

1. Show top 8 recent launches
2. One-click pin to LaunchGrid

### 5.4 QuickActions

1. Lock screen
2. Restart
3. Shutdown
4. Open system settings

## 6. Frontier Feature Pack (Wave 2 Core)

### 6.1 Command Palette+

Add advanced command prefixes:

1. `ws:` quick website action
2. `url:` open direct URL
3. `calc:` inline expression evaluation

### 6.2 Smart Ranking 2.0

Ranking factors:

1. Historical frequency
2. Recency
3. Time-of-day affinity (morning/evening behavior weighting)

### 6.3 Flow Launch

One command can execute multiple steps:

1. Open browser app
2. Open a configured set of URLs
3. Open configured folders/files

### 6.4 Context Actions

Each launch card includes context actions:

1. Run as administrator
2. Open target directory
3. Copy target path

### 6.5 Theme Engine v1

Theme profile controls:

1. Color tokens
2. Typographic scale
3. Radius/shadow/material profile

## 7. Architecture Impact

Extend current modules:

1. `Shell`: Neo-Panel layout zones and orchestration
2. `Search`: command prefix router + richer ranking inputs
3. `Launch`: context actions + flow execution pipeline
4. `Theme`: tokenized theme engine implementation
5. `Storage`: persist flows, theme profile, ranking metadata

## 8. Performance and UX Targets

Wave 2 acceptance thresholds:

1. Shell toggle to first interactive frame: `<=120ms`
2. Command suggestion refresh (common case): `<=80ms`
3. Command palette action execution dispatch: `<=100ms`

## 9. Quality and Validation

Testing focus:

1. Command prefix parser and execution routing
2. Ranking 2.0 deterministic scoring tests
3. Flow execution ordering and error rollback behavior
4. UI integration tests for shell visibility and panel interaction

Verification gate:

1. `dotnet test` all green
2. `dotnet build -c Release` success
3. `scripts/verify.ps1` success

## 10. Non-Goals for This Wave

1. Cloud sync
2. Plugin marketplace runtime
3. Cross-platform support
