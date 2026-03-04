# VStart Next AI Agent Design (Wave 3)

Date: 2026-03-04
Status: Approved
Priority: Surpass legacy launchers with deep AI-native capability

## 1. Objective

Build an AI-native Windows launcher that is clearly ahead of classic launcher tools in:

1. Natural language task execution
2. Multi-step autonomous planning and tool orchestration
3. Bilingual user experience (Chinese and English)
4. Safe, auditable automation

## 2. Product Strategy

Use a vertical-first strategy:

1. Build a launcher-native vertical agent first (high reliability, fast delivery)
2. Keep architecture extensible for future general desktop agent capabilities

This avoids over-scoping while preserving a path toward "OpenClaw-style" multi-step agents.

## 3. AI Model Strategy

1. Cloud model first (primary inference path)
2. Optional lightweight local model for intent pre-classification/offline fallback
3. Model router controls provider selection and degradation behavior

Rationale:

1. User hardware constraints are respected
2. Higher initial intelligence and faster iteration
3. Local model can improve resilience without becoming a hard dependency

## 4. System Architecture

### 4.1 Core Components

1. `AgentOrchestrator`
   - Owns execution lifecycle, retries, state transitions
2. `AgentPlanner` (LLM-backed)
   - Converts natural language into structured action plans
3. `ToolRegistry`
   - Registers callable tools and schemas
4. `PolicyGuard`
   - Validates risk, permissions, and arguments before execution
5. `AgentExecutor`
   - Executes plan steps, supports dynamic replanning from step outputs
6. `AgentMemory`
   - Stores short-term conversation context and long-term preference profile
7. `ModelRouter`
   - Routes calls to cloud or fallback local model
8. `ExecutionAudit`
   - Stores plan and execution trace for replay/debugging

### 4.2 Initial Tool Surface

1. `launch_app`
2. `open_url`
3. `open_path`
4. `run_flow`
5. `quick_action`

These map to existing launcher capabilities and preserve deterministic execution.

## 5. Interaction and Data Flow

1. User enters natural language command in command bar
2. Intent precheck decides `classic` vs `agent` path
3. Context builder assembles available tools, session state, and preference hints
4. Planner outputs structured JSON plan
5. Policy guard validates each step
6. Executor runs tools and collects outcomes
7. Orchestrator replans if needed (on partial failures or unexpected outputs)
8. Result synthesizer generates final user-facing message
9. Audit log persists input-plan-execution-result chain

## 6. Intelligence Features (Wave 3 Baseline)

1. Multi-step planning with dependency-aware sequencing
2. Self-check reflection pass before execution (accepted +300-600ms latency tradeoff)
3. Tool feedback loop with adaptive replanning
4. Long-term preference learning (time slots, app habits, language preference)
5. Confidence gating (ask clarifying questions when uncertain)

## 7. Bilingual Support (zh-CN / en-US)

1. Runtime UI language switching (no restart)
2. Mixed-language command understanding
3. Response language defaults to input language, with override option to follow UI language
4. Shared structured JSON plan layer to keep execution language-agnostic
5. Dual-language evaluation datasets (Chinese + English)

## 8. Security and Trust Boundaries

1. Risk levels: `low`, `medium`, `high`
2. Mandatory confirmation for high-risk actions:
   - Shutdown/restart
   - Admin privilege escalation
   - Potentially destructive bulk operations
3. Strict tool whitelist; no arbitrary free-form command execution by model output
4. JSON schema validation for all tool arguments
5. Prompt injection defense by separating untrusted content from system policy prompts
6. Least-privilege execution defaults
7. Sensitive field redaction in logs
8. Fallback to classic launcher path when AI services are unavailable

## 9. Performance and Quality Targets

Wave 3 acceptance thresholds:

1. Agent plan generation `P95 <= 1.2s`
2. End-to-end single-step execution `P95 <= 2.0s`
3. End-to-end 3-step execution `P95 <= 4.5s`
4. Natural language task success rate `>= 85%` on top-50 task set
5. High-risk mis-execution: `0`
6. Classic path regression: `0` critical regressions

## 10. Milestones

1. `M1 - Agent Core`
   - Orchestrator, planner contract, tool registry, policy guard
2. `M2 - Intelligent Execution`
   - Multi-step execution, retries, adaptive replanning, UI status integration
3. `M3 - Personalization and Evaluation`
   - Memory profile + benchmark harness
4. `M4 - Local Model Assist`
   - Lightweight local pre-classifier/offline partial fallback

## 11. Non-Goals (Current Wave)

1. Full autonomous web-browsing agent
2. Cross-platform desktop support
3. Plugin marketplace runtime

