<manifest requires="CS.01, CS.03, CS.07, OR.01, OR.03, AP.02, AP.04, AP.05, AP.06, BB.05, BB.06, BB.07, BB.08" allowed="VK.Blocks.Core, VK.Blocks.AI, VK.Blocks.Resilience, Microsoft.Extensions.AI*, Microsoft.Extensions.DependencyInjection.Abstractions, Microsoft.Extensions.Http, Microsoft.Extensions.Logging.Abstractions, Microsoft.Extensions.Options" forbidden="VK.Blocks.AI.Psyche, VK.Blocks.AI.Corpus, VK.Blocks.AI.Engram, VK.Blocks.AI.Eidos, VK.Blocks.AI.AgentFramework, VK.Blocks.AI.Cortex, VK.Blocks.AI.Synapse">

# VK.Blocks AI.MEAI Manifest

Implements the `VK.Blocks.AI` execution ports over `Microsoft.Extensions.AI` (MEAI) for a single stateless logical model call and its per-call governance; delegates shared contracts, connection health/routing, and durable persistence to owning modules.

## 1. Ownership
- **Owns**: OWN-MODEL-CALL, OWN-PROVIDER-TRANSLATION, OWN-TRANSPORT-RESILIENCE, OWN-FAILOVER-EXECUTION, OWN-USAGE-ACTUAL, OWN-CALL-IDEMPOTENCY, OWN-TOOL-LOOP, OWN-TOOL-FILTER, OWN-OUTBOUND-GUARD, OWN-REQUEST-CAP
- **Delegates**: OWN-PORT-CONTRACT → AI, OWN-PORT-ERRORS → AI, OWN-CATALOG → AI, OWN-TOOL-CONTRACT → AI, OWN-TOKEN-COUNT → AI, OWN-ROUTE → AI.Synapse, OWN-CONN-HEALTH → AI.Synapse, OWN-AUDIT-PERSISTENCE → Host

## 2. Boundaries
- **Non-goals**: Conversation state (`Psyche`/`Echo`), memory (`Engram`), knowledge (`Corpus`), output contracts (`Eidos`), agent orchestration (`AgentFramework`), turn orchestration (`Cortex`), connection health/circuit state/routing (`Synapse`), and durable audit persistence (`Host`).
- Depends only on `VK.Blocks.AI`, `VK.Blocks.Core`, and shared infrastructure (`Resilience`, `MEAI`); never depends on functional AI libraries or `AgentFramework`.
- Provider-specific schema and tool translation lives exclusively here and nowhere upstream (`Excludes: AI.Eidos`).

## 3. Public Surface
- **Provided-Tier Engine Implementations**: Stateless `MEAI`-backed implementations of the `VK.Blocks.AI` chat, embedding, audio, and image ports, registered via explicit builder/slice APIs (`Provided` tier replacing `Fallback` defaults).
- **Per-Provider & Capability Slices**: Independent capability slices (chat/embedding/audio/image) with cascade-disable and explicit provider registration.
- **Zero-SDK-Leakage Surface**: No vendor SDK or `MEAI` types exposed on the public surface; consumers interact solely through `VK.Blocks.AI` ports and typed options.

## 4. Defaults & Overrides
- Replaces `VK.Blocks.AI` `NoOp` / `Fallback` defaults at the `Provided` tier without registration-order sensitivity; governance stores and detector contracts fall back to `AI` defaults when not overridden by `Host`.

## 5. Invariants
### Stateless Execution & Routing
- **MEA-01** MUST remain stateless across calls: owns no conversation, thread, memory, or planning; the only permitted loop is tool invocation within a single logical model call (`OWN-TOOL-LOOP`).
- **MEA-02** MUST resolve providers explicitly through a provider-client seam and fail at startup validation if a selected provider is unregistered; providers sharing a wire protocol share one adapter differing by configuration.

### Governance Pipeline & Limits
- **MEA-03** MUST enforce a fixed onion order in the governance pipeline (outermost: OpenTelemetry/Audit, then Idempotency, FunctionInvocation, OutboundGuard & RequestCap, Routing/Failover (OWN-FAILOVER-EXECUTION), and innermost: Per-Connection Transport); only enable/disable is configurable, never reordering.
- **MEA-04** MUST enforce `OWN-REQUEST-CAP` as a hard limit on a single model request, returning a typed error on exceedance and never silently truncating.
- **MEA-05** MUST ensure `OWN-TOOL-FILTER` is the single execution point for tool filtering and auditing across both direct calls and agent runtimes, and scope `OWN-CALL-IDEMPOTENCY` to the logical model call rather than individual tool-loop iterations.

### Resilience & Usage
- **MEA-06** MUST route all outbound provider traffic through `VK.Blocks.Resilience` named transport policies (retry with exponential backoff + jitter, per-attempt timeout) with SDK built-in retries disabled, and MUST NOT hold circuit-breaker state (`OWN-CONN-HEALTH` belongs to `AI.Synapse`) or duplicate upper-layer retries.
- **MEA-07** MUST derive `OWN-USAGE-ACTUAL` strictly from provider responses as the single source of truth for actual usage, report it to `AI.Synapse` via `IVKAIInvocationObserver`, and use `VK.Blocks.AI` (`OWN-TOKEN-COUNT`) only for pre-call estimation.
- **MEA-08** MUST derive `ActivitySource`/`Meter` from the block identifier and exclude raw prompts and completions from telemetry by default.

</manifest>
