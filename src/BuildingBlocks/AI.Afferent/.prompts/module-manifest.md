---
layer: 3
id: ai-afferent-manifest
scope: building-blocks/ai-afferent
requires: CS.01, CS.03, AP.01, AP.05, BB.06, OR.01, OR.03
---

# VK.Blocks AI.Afferent Manifest (Layer 3)

The ingress gateway — single entry point for all external signals (text, audio, perception, system events) before the cognitive core. Normalizes, validates, budgets, and sanitizes input through a deterministic `BeforePipelineStage` chain.

## Architectural Boundaries

### 1. Stage Ordering
- Stages execute in a fixed deterministic order. Reordering requires an ADR (`DL.03`).
- Guardrails MUST always run first (security boundary).

### 2. Failure Semantics
- **Guardrails / Text / Audio → Fail-Closed**: Violations and processing errors MUST propagate as `VKResult.Failure`.
- **Environment / Sensors → Fail-Open**: Supplementary perception failures MUST NOT block the pipeline. Log at Warning level only.

### 3. Guardrail Chain
- Execution order: Content Moderation → Injection Detection → PII Masking.
- PII Masking is the ONLY step permitted to mutate input content. All other checks are read-only.

### 4. Token Budget
- Dual-threshold: soft warning + hard rejection. Hard limit terminates the pipeline — no silent truncation.
- Token counting MUST delegate to `IVKTokenCounter`. Direct estimation is PROHIBITED.

### 5. Stream Ownership
- Audio stages MUST NOT dispose input streams — ownership belongs to the caller.

