<manifest requires="CS.01, CS.03, CS.06, AP.01, AP.02, AP.05, BB.02, BB.04, BB.06" allowed="VK.Blocks.Core, VK.Blocks.AI, VK.Blocks.Resilience, Microsoft.Extensions.DependencyInjection.Abstractions, Microsoft.Extensions.Options, Microsoft.Extensions.Logging.Abstractions, System.Threading.RateLimiting" forbidden="VK.Blocks.AI.MEAI*, VK.Blocks.AI.Cortex, VK.Blocks.AI.AgentFramework, OpenAI, Azure.AI.OpenAI, OllamaSharp, Mscc.GenerativeAI, Microsoft.Extensions.AI*, Microsoft.Extensions.Http, Microsoft.EntityFrameworkCore*, StackExchange.Redis*">

# VK.Blocks AI.Synapse Manifest

Owns in-memory cross-call model governance policy and state (routing, connection health, provider capacity, credential references, caller quotas, and cost); delegates execution, transport resilience, underlying resilience primitives, actual usage determination, token counting, static catalog facts, and secret storage.

## 1. Ownership
- **Owns**: OWN-ROUTE, OWN-CONN-HEALTH, OWN-PROVIDER-CAPACITY, OWN-CREDENTIAL-REF, OWN-QUOTA, OWN-CUMULATIVE-QUOTA, OWN-COST
- **Delegates**: OWN-PORT-CONTRACT → AI, OWN-PORT-ERRORS → AI, OWN-CATALOG → AI, OWN-TOKEN-COUNT → AI, OWN-MODEL-CALL → AI.MEAI, OWN-USAGE-ACTUAL → AI.MEAI, OWN-TRANSPORT-RESILIENCE → AI.MEAI, OWN-FAILOVER-EXECUTION → AI.MEAI, OWN-SECRET-STORE → Host

## 2. Boundaries
- **Non-goals**: Transport retry/timeout/circuit execution, failover loop execution, HTTP client/transport wiring, token counting/estimation, actual usage determination, in-process wait queuing for quota replenishment, turn deadline orchestration, content safety, secret storage, billing/invoicing, and durable persistence (reconsider never — owned by peers or `Host`).
- **Primitive Reuse & Seam Split**: Reuses `VK.Blocks.Resilience` report-driven circuit state machines and rate-limiting primitives while owning AI governance semantics; cross-module seams (`IVKAIRoutingPolicy`, `IVKAIInvocationObserver`, `IVKAICredentialResolver`) live in `AI`, while governance-internal store/policy ports live here.
- **Split Trigger**: Routing cluster (`OWN-ROUTE`, `OWN-CONN-HEALTH`, `OWN-PROVIDER-CAPACITY`, `OWN-CREDENTIAL-REF`) and Budget cluster (`OWN-QUOTA`, `OWN-CUMULATIVE-QUOTA`, `OWN-COST`) coexist as pure in-memory policy and state; extract either cluster if it acquires concrete persistence dependencies (e.g., EF Core, Redis).
- **Input & Context Purity**: Never reads message content or estimates tokens; operates solely on logical profiles, declared capability requirements, caller-supplied token estimates (derived upstream via `OWN-TOKEN-COUNT`), an opaque scope key, an application-owned affinity key, and correlation tags without tenant/user business semantics.

## 3. Public Surface
- `Provided`-tier implementations of `AI` routing decision, invocation observation, and credential-reference resolution seams.
- Pluggable routing strategy (priority, cost, latency, affinity), connection registry, health evaluation, and provider capacity (RPM/TPM/concurrency) abstractions.
- Caller-side token budget reservation/settlement and cumulative quota (token/cost cap) abstractions.
- Time-versioned pricing source, exact-decimal cost calculator, and idempotent cost ledger abstractions, plus static endpoint security validator.
- Explicit `Override*` builder extensions for replacing built-in stores and policies deterministically.

## 4. Defaults & Overrides
- Ships bounded, single-node in-memory defaults (`Provided` tier replacing `AI` `NoOp` policy/observer/resolver defaults); multi-replica deployments without distributed store overrides scale effective quota and capacity ceilings linearly with replica count.
- Production hosts override storage, ledger, pricing, and secret resolution ports via explicit builder overrides without registration-order sensitivity.

## 5. Invariants
### Collaboration
- **SYN-01** Observation MUST never alter the outcome of the call it observes or block lease release; its failures are contained and never propagate to the caller.
- **SYN-02** Every attempt is reported with its logical invocation and attempt identity; consumers deduplicate on that pair.

### Routing & Health
- **SYN-03** When routing is enabled, callers reference logical profiles, never concrete connections or model IDs, and misconfiguration fails at startup; when routing is disabled (e.g., gateway cost-only mode), routing profile and connection registry startup requirements are bypassed.
- **SYN-04** Candidate filtering is ordered: enabled, Catalog capability facts (including deprecation), region/residency, health (circuit and cooldown state), then provider capacity (RPM/TPM/concurrency); unknown capabilities are not treated as unsupported.
- **SYN-05** Routing operates per model call inside the tool-invocation loop so failed calls never re-execute completed tool side effects; application-scoped session affinity pins subsequent calls in a turn to the first succeeding connection, unpinning and failing over to the next candidate only when a subsequent call fails before streaming output commits its first chunk.
- **SYN-06** Health is tracked per connection (never per provider or globally) from reported outcomes (latency, time-to-first-token, and failure kind); invalid-request and content-rejection never trigger a switch or trip health, rate-limit honors retry-after cooldown monotonically, and auth failure disables the connection until reconfigured.

### Quota & Cost
- **SYN-07** Caller-side budgets (reserved at invocation start from caller-supplied input/max-output estimates, settled at end with actual usage by caller scope key) and provider-side capacity (checked per candidate per attempt by connection identity) never share a lease or key space; pre-call evaluation is atomic and all reservations/leases MUST release deterministically on completion, fault, cancellation, or stream interruption with TTL expiry and idempotent release/settle.
- **SYN-08** Both rate budgets and cumulative quotas (token/cost ceilings) evaluate before candidate dispatch; over-limit behavior supports only explicit rejection (returning typed outcome with `RetryAfterMs` metadata, never queuing in-process) or single-step profile downgrade (which MUST re-validate target Catalog capabilities and fail explicitly with `CapabilityMismatchOnDowngrade` if unsatisfied), never throwing exceptions.
- **SYN-09** Prices are versioned by effective date using exact decimal arithmetic (input/output/cached/reasoning tokens); missing prices or non-token modalities without explicit token-pricing mappings in v1 yield an explicit `Unpriced` result, never zero or guessed, while provider capacity (`RPM`/concurrency) still guards all modalities.
- **SYN-10** Cost accounting is idempotent per invocation-and-attempt identity and aggregates by opaque scope key and correlation tags; high-cardinality keys (`ScopeKey`, `AffinityKey`) are restricted to state partitioning, ledger records, logs, and traces, and MUST NOT be emitted as OpenTelemetry `Meter` tags.

### Security
- **SYN-11** Connections hold credential references, never secret material; secret material never appears in logs, events, errors, or diagnostics.
- **SYN-12** Endpoints are statically validated at connection registration and endpoint resolution: secure transport (`https`) only, rejecting private and link-local targets without performing network I/O.

## 6. Registration & Diagnostics
- Routing, quota, and cost are independently enable-able with cascade-disable; gateway mode disables routing (`VKRoutingOptions.Enabled = false`) and quota while keeping cost observation active.
- When routing is disabled, the executor behaves identically to running without routing while still reporting usage to cost observation when enabled.

</manifest>
