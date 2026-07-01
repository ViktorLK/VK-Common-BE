---
layer: 3
id: ai-efferent-manifest
scope: building-blocks/ai-efferent
# [Specialized Rules for AI.Efferent] Egress Pipeline, Tool Dispatch, Output Guardrails, and Streaming Delivery
requires: CS.01, CS.03, AP.01, AP.03, AP.05, BB.01, BB.03, BB.06, BB.07, OR.01, OR.03
---

# 📡 VK.Blocks AI.Efferent Manifest (Layer 3)

The `AI.Efferent` module is the system's efferent gateway — the symmetric counterpart to `AI.Afferent`. It governs all output processing after the cognitive core (Psyche) completes reasoning: output safety validation, text formatting, tool/action dispatch, TTS synthesis, and token accounting. Every `IVKPsycheAfterPipelineStage` implementation operates on `context.Response.ChatResponse` to sanitize, transform, and deliver the final response.

## Specialized Rules for Efferent

### 1. Deterministic Egress Stage Ordering
- **Fixed Schedule Contract**: Each `IVKPsycheAfterPipelineStage` implementation MUST declare a deterministic `VKPipelineStageSchedule` order index. The canonical execution order is:
  1. **Guardrails** (100) — Output content moderation, data-leak prevention (DLP)
  2. **Text** (200) — Markdown sanitization, whitespace trimming, channel-specific formatting
  3. **Actuators** (300) — Tool invocation dispatch (AI.Praxis decisions → execution)
  4. **Audio** (500) — Text-to-Speech synthesis
  5. **Tokenics** (600) — Output token counting and cost accounting
- **Security-First Ordering**: Guardrails MUST always execute before any other stage. Output that fails safety validation MUST NOT reach Actuators, Audio, or delivery. No reordering without ADR (`DL.03`).

### 2. Egress Guardrail Semantics (Fail-Closed)
- **Two-Layer Defense**: The default `IVKEgressGuardrail` implementation MUST execute in strict order: (1) Content Moderation → (2) Data Leak Prevention (PII masking).
- **No Injection Detection**: Unlike Afferent guardrails, Efferent does NOT include injection detection — it validates the system's own output, not untrusted user input.
- **Response Mutation via `with` Expression**: When guardrails modify output content, the pipeline stage MUST create a new `Message` via record `with` expression and update `context.Response.ChatResponse` immutably. Direct mutation of `Message.Content` is PROHIBITED.
- **BlockOnViolation Default**: `BlockOnViolation` defaults to `true`. When active, any content policy violation MUST return `VKResult.Failure` and prevent the response from reaching the user.

### 3. Actuator Dispatch — Judgment/Execution Separation
- **Praxis Decides, Efferent Executes**: `EgressActuatorsPipelineStage` receives `VKToolCall` objects from `context.Response.ChatResponse.Message.ToolCalls`. It MUST NOT decide which tools to call — that responsibility belongs entirely to AI.Praxis. Efferent only executes.
- **Dispatch Contract**: `IVKEgressActuators.DispatchActionsAsync` takes `IReadOnlyList<VKToolCall>` and returns `IReadOnlyList<VKToolResult>`. Each `VKToolResult` MUST include the originating `CallId` for correlation.
- **Tool Result Propagation**: Successful execution results MUST be written to `context.SetState(executionResult.Value)` for downstream consumption or return-loop to the agent.
- **No-Op Guard**: If `context.Response.ChatResponse.Message.ToolCalls` is null or empty, the stage MUST return `VKResult.Success()` immediately without invoking the dispatcher.

### 4. Text Formatting & Channel Adaptation
- **Formatter Contract**: `IVKEgressTextFormatter.FormatOutputAsync` is the extension point for channel-specific formatting (Discord Markdown, LINE plain text, API JSON, etc.).
- **Immutable Response Update**: Like guardrails, text formatting MUST update `context.Response.ChatResponse` using record `with` expressions. The original response MUST NOT be mutated in place.
- **Default Behavior**: The default implementation performs whitespace trimming only. Markdown sanitization (`SanitizeMarkdown`) is opt-in via `VKEgressTextOptions`.

### 5. Audio Synthesis Lifecycle
- **Non-Blocking by Default**: `EgressAudioPipelineStage` defaults to `Enabled = false`. When enabled, TTS failures MUST be logged at Warning level but MUST NOT block the pipeline — text delivery takes priority over audio.
- **Stream Output**: Successful synthesis writes a `Stream` to `context.SetState<Stream>()`. The caller is responsible for stream consumption and disposal.
- **Asymmetry with Afferent**: Afferent Audio is fail-closed (transcription is the primary input). Efferent Audio is fail-open (synthesis is supplementary output).

### 6. Output Token Accounting
- **Metering, Not Enforcement**: Unlike Afferent tokenics which enforces hard budget limits, Egress tokenics is purely observational — it counts output tokens via `IVKTokenCounter` and accumulates them in `context.Response.TotalEstimatedTokens`.
- **Non-Fatal Failures**: Token counting failures MUST be caught and logged at Warning level. They MUST NOT propagate as pipeline failures.
- **Token Counter Dependency**: Output token counting MUST delegate to `IVKTokenCounter` (from `AI` module). Direct estimation is PROHIBITED.
