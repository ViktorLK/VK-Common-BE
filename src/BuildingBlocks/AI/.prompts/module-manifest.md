<manifest id="ai-manifest" layer="Tier 2 — Domain Port" scope="building-blocks/ai" requires="CS.01, CS.03, AP.01, AP.05, AP.06, BB.02">

# VK.Blocks AI Manifest (Layer 3)

Pure, provider-agnostic contracts for AI cognitive capabilities (chat, text, speech, transcription, image generation, agents, tools, governance). Concrete provider wiring is delegated to `VK.Blocks.AI.Extensions` and `VK.Blocks.AI.Agents`.

## Architectural Boundaries

### 1. Zero-SDK Port Purity (ADR-020)
- This package depends only on `VK.Blocks.Core` and `Microsoft.Extensions.*` abstractions.
- `Microsoft.SemanticKernel`, `Microsoft.Extensions.AI`, and all provider SDKs (OpenAI, Azure, Ollama, Google) are FORBIDDEN. Provider types MUST NOT leak into ports, models, or errors.

### 2. Result-Only Contracts
- Every port returns `VKResult<T>`; streaming ports return `IAsyncEnumerable<VKResult<T>>` with `CancellationToken` propagated end to end.
- Foreseeable provider failures are surfaced as centrally defined AI error constants — never as thrown exceptions or bare nulls.

### 3. Co-Located NoOp Defaults
- Every engine/governance port MUST ship a NoOp fallback in this package, registered idempotently via `TryAdd`.
- Replacing a fallback requires an explicit builder override from a provider package; resolution MUST NOT depend on registration order. No silent fallbacks.

### 4. Contractual Args Mapping (AP.05 Specialized)
- Per-call `Args` records MUST be source-generated from Options annotated with `[VKRequestOverride]`.
- **Security by Default**: Options properties not opted in are excluded from the generated Args surface.

### 5. Perimeter Cross-Cutting
- Resilience (Polly), governance, and instrumentation attach as decorators around ports at the provider boundary — never inside contracts or domain models.

</manifest>
