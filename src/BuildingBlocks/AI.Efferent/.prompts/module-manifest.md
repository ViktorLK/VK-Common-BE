---
layer: 3
id: ai-efferent-manifest
scope: building-blocks/ai-efferent
requires: CS.01, CS.03, AP.01, AP.05, BB.06, OR.01, OR.03
---

# VK.Blocks AI.Efferent Manifest (Layer 3)

The egress gateway — symmetric counterpart to AI.Afferent. Governs all output processing after the cognitive core completes reasoning: safety validation, text formatting, tool dispatch, TTS synthesis, and token accounting through a deterministic `AfterPipelineStage` chain.

## Architectural Boundaries

### 1. Stage Ordering
- Stages execute in a fixed deterministic order. Reordering requires an ADR (`DL.03`).
- Guardrails MUST always run first — unsafe output MUST NOT reach actuators, audio, or delivery.

### 2. Failure Semantics
- **Guardrails / Text → Fail-Closed**: Output policy violations MUST propagate as `VKResult.Failure`.
- **Audio → Fail-Open**: TTS synthesis is supplementary. Failures log at Warning level but MUST NOT block text delivery.
- **Token Accounting → Fail-Open**: Output token counting is observational only — failures MUST NOT propagate.

### 3. Judgment / Execution Separation
- Efferent EXECUTES tool calls; it MUST NOT DECIDE which tools to call — that responsibility belongs to AI.Praxis.
- No tool calls in the response → stage returns `VKResult.Success()` immediately (no-op guard).

### 4. Guardrail Scope
- Egress guardrails: Content Moderation → Data Leak Prevention (PII masking). No injection detection (unlike Afferent — this validates system output, not untrusted input).
- Response mutation MUST use immutable record `with` expressions. Direct mutation of response content is PROHIBITED.

### 5. Token Counting
- Token counting MUST delegate to `IVKTokenCounter`. Direct estimation is PROHIBITED.

### 6. Stream Ownership
- TTS output streams belong to the caller. Efferent stages MUST NOT dispose them.

