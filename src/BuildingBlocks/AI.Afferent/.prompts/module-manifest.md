---
layer: 3
id: ai-afferent-manifest
scope: building-blocks/ai-afferent
# [Specialized Rules for AI.Afferent] Ingress Pipeline, Guardrail Chain, and Token Budget Enforcement
requires: CS.01, CS.03, AP.01, AP.03, AP.05, BB.01, BB.03, BB.06, BB.07, OR.01, OR.03
---

# 🛡️ VK.Blocks AI.Afferent Manifest (Layer 3)

The `AI.Afferent` module is the system's afferent gateway — the single entry point for all external signals (text, audio, environment perception, system events) before they reach the cognitive core. It normalizes, validates, budgets, and sanitizes every input through a deterministic chain of `IVKPsycheBeforePipelineStage` implementations.

## Specialized Rules for Afferent

### 1. Deterministic Ingress Stage Ordering
- **Fixed Schedule Contract**: Each `IVKPsycheBeforePipelineStage` implementation MUST declare a deterministic `VKPipelineStageSchedule` order index. The canonical execution order is:
  1. **Guardrails** (100) — Content moderation, injection detection, PII masking
  2. **Environment** (200) — Screen/window perception capture
  3. **Sensors** (250) — System event queue consumption
  4. **Text** (300) — Unicode normalization, whitespace trimming, text splitting
  5. **Audio** (500) — Audio stream transcription to text
  6. **Tokenics** (600) — Token counting, budget enforcement
  7. **RateLimit** (700) — Cost/rate throttling
- **No Reordering Without ADR**: Changing an order index constitutes an architectural decision and MUST be preceded by an ADR (`DL.03`).

### 2. Fail-Open vs. Fail-Closed Semantics
- **Guardrails = Fail-Closed (default)**: `IngressGuardrailsPipelineStage` MUST propagate `VKResult.Failure` upstream when `BlockOnViolation` is `true`. This is the security boundary — silent pass-through of violations is PROHIBITED.
- **Environment & Sensors = Fail-Open**: `EnvironmentPipelineStage` and `IngressSensorsPipelineStage` MUST return `VKResult.Success()` even on provider failure, since perception data is supplementary. Failures MUST be logged at Warning level but never block the pipeline.
- **Audio & Text = Fail-Closed**: Transcription and text processing failures MUST propagate as `VKResult.Failure` since they represent corruption of the primary input signal.

### 3. Guardrail Chain Composition
- **Three-Layer Defense**: The default `IVKIngressGuardrail` implementation MUST execute in strict order: (1) Content Moderation → (2) Prompt Injection Detection → (3) PII Masking.
- **PII Masking is Output-Mutating**: Privacy filtering (`IVKPrivacyFilter.MaskAsync`) is the ONLY guardrail step that may mutate the text content. Moderation and injection detection are read-only checks.
- **State Propagation**: When guardrails modify input (e.g., PII masking), the sanitized text MUST be written to `VKPsycheContext` via `SetState<string>()` for downstream stages to consume.

### 4. Token Budget Enforcement
- **Dual-Threshold Model**: `IngressTokenicsPipelineStage` MUST implement both a soft warning threshold (`BudgetWarningThreshold`, default 80%) and a hard rejection limit (`MaxInputTokens`, default 32768).
- **Hard Limit = Pipeline Termination**: When `EnforceHardLimit` is `true` and token count exceeds `MaxInputTokens`, the stage MUST return `VKResult.Failure` with `IngressTokenicsErrors.BudgetExceeded`. No truncation — the caller must reduce input.
- **Token Counter Dependency**: Token counting MUST delegate to `IVKTokenCounter` (from `AI` module). Direct token estimation or regex-based approximation is PROHIBITED.

### 5. Audio Stream Lifecycle
- **Stream Ownership**: `IngressAudioPipelineStage` reads the audio `Stream` from `VKPsycheContext.State<Stream>()`. The stage MUST NOT dispose the stream — ownership belongs to the caller.
- **Transcription Output**: Successful transcription MUST overwrite `context.State<string>()` with the text result, making it available to downstream text/tokenics stages.
- **Duration Limits**: Audio processing MUST respect `MaxAudioDurationSeconds` (default 600s) to prevent unbounded processing time.

### 6. Environment Perception Contract
- **Non-Blocking Capture**: `IVKEnvironmentPerceptionProvider.GetEnvironmentStateAsync` MUST return quickly (< 500ms). Heavy OCR or screen-capture operations SHOULD be offloaded to background tasks and cached.
- **State Record Immutability**: `VKEnvironmentState` is a `record` and MUST remain immutable after creation. No post-capture mutation.
