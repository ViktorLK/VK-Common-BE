---
layer: 3
id: ai-efferent-manifest
scope: building-blocks/ai-efferent
requires: CS.01, CS.03, AP.01, AP.05, BB.06, OR.01, OR.03
---

# VK.Blocks AI.Efferent Manifest (Layer 3)

The egress gateway — symmetric counterpart to AI.Afferent. Governs all output processing, presentation, actuation, and delivery after the cognitive core completes reasoning: safety validation, streaming traffic shaping, multimodal expression, tool actuation, TTS synthesis, interaction telemetry, and token accounting through a deterministic `AfterPipelineStage` chain.

## Architectural Boundaries

### 1. Stage Ordering
- Stages execute in a fixed deterministic order. Reordering requires an ADR (`DL.03`).
- Guardrails MUST always run first — unsafe output MUST NOT reach actuators, audio, or delivery.

### 2. Failure Semantics
- **Guardrails / Text → Fail-Closed**: Output policy violations MUST propagate as `VKResult.Failure`.
- **Audio → Fail-Open**: TTS synthesis is supplementary. Failures log at Warning level but MUST NOT block text delivery.
- **Token Accounting → Fail-Open**: Output token counting is observational only — failures MUST NOT propagate.
- **Actuators (Tools) → Fail-Closed with Trace**: Execution errors MUST propagate with detailed error codes.

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

### 7. Streaming Egress & Traffic Shaping
- **Jitter Buffer & Pacing**: Output rate limiting and token jitter smoothing MUST be decoupled from business layers.
- **Sentence / Utterance Chunking**: Stream chunking for real-time TTS MUST split on semantic punctuation boundaries (`.`, `!`, `?`, `\n`) to achieve low-latency audio pipelining.
- **Sliding-Window Redaction**: Streaming safety checks MUST buffer a minimal token window to intercept and mask sensitive patterns before emitting to callers.

### 8. Multimodal Expression & Alignment
- **Prosody & Emotion Mapping**: Affective tokens MUST map cleanly to TTS engine parameters (Pitch, Rate, SSML) without hardcoded vendor dependencies.
- **Avatar & Viseme Sync**: When producing audio, time-aligned visemes and facial blendshape streams MUST align with output timestamps.
- **No Domain Reflection**: Reflective sniffing of specific domain properties (e.g., `DialogueSegments`, `NarrativeText`) is STRICTLY PROHIBITED (`AP.07`). Stages MUST interact exclusively through strongly typed contracts (`IVKSegmentedResponse`) or defer domain extraction to App layers.

### 9. Actuation & Side-Effect Safety
- **Idempotency Enforcement**: External tool executions with mutations MUST carry deterministic idempotency keys.
- **Human-in-the-Loop (HITL) Gate**: High-risk actions (e.g., deletions, financial ops) MUST support execution suspension with an `InterruptionToken` awaiting explicit user confirmation.
- **Compensating Actions**: Multi-step tool executions MUST provide rollback / saga compensations on partial failures.

### 10. Interaction Interruption & Delivery Telemetry
- **Barge-in Support**: Immediate cessation of in-flight TTS synthesis and token streams upon receiving an interruption signal.
- **Actual Delivery Watermark**: Efferent MUST track and report the actual delivered token/text position to the cognitive core (AI.Psyche/AI.Engram) rather than the total LLM-generated length.

### 11. Compliance & AI Watermarking
- **Provenance & Watermarking**: Invisible or cryptographic digital watermarking (e.g., zero-width steganography for text, inaudible acoustic watermarking for speech) MUST be applied at the final egress stage.
- **Regulatory Disclaimers**: Automatic injection of mandatory jurisdiction-specific AI disclosures where required by compliance policy.


