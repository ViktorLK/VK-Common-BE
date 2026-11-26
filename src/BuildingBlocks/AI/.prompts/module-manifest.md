<manifest requires="CS.01, CS.03, AP.01, AP.02, AP.04, AP.05, AP.06, BB.02, BB.05, BB.06, BB.07, BB.08" allowed="VK.Blocks.Core, Microsoft.Extensions.DependencyInjection.Abstractions, Microsoft.Extensions.Logging.Abstractions, Microsoft.Extensions.Options" forbidden="VK.Blocks.AI.*, VK.Blocks.Resilience, Microsoft.Extensions.Http, Microsoft.Extensions.AI*, Microsoft.Agents.AI*, Microsoft.SemanticKernel*, OpenAI*, Azure*, Ollama*, Google*, Anthropic*, Mscc.*">

# VK.Blocks AI Manifest

Owns zero-SDK AI execution and agent ports, cross-module governance and context seams, shared error/usage vocabularies, static model catalog, atomic tool contracts, and approximate token counting; delegates execution, agent runtimes, and stateful governance to downstream modules.

## 1. Ownership
- **Owns**: OWN-PORT-CONTRACT, OWN-PORT-ERRORS, OWN-CATALOG, OWN-TOOL-CONTRACT, OWN-TOKEN-COUNT
- **Delegates**: OWN-MODEL-CALL → AI.MEAI, OWN-PROVIDER-TRANSLATION → AI.MEAI, OWN-TOOL-LOOP → AI.MEAI, OWN-TOOL-FILTER → AI.MEAI, OWN-AGENT-RUN → AI.AgentFramework, OWN-AGENT-GROUP → AI.AgentFramework, OWN-AGENT-WORKFLOW → AI.AgentFramework, OWN-ROUTE → AI.Synapse, OWN-CONN-HEALTH → AI.Synapse, OWN-QUOTA → AI.Synapse, OWN-COST → AI.Synapse, OWN-CREDENTIAL-REF → AI.Synapse

## 2. Boundaries
- **Seam Criterion**: A contract belongs here ONLY if it abstracts a heavy external AI/Agent SDK or serves as a cross-module seam between non-referencing peers (`AI.MEAI` ↔ `AI.Synapse` routing/observation/credential-ref seams, `AI.Cortex` ↔ `AI.AgentFramework` context-provider seam, shared guardrail detector contracts).
- **Non-goals**: Concrete model invocation, HTTP transport/client configuration, rate limiting, token budgeting, cost calculation/pricing (legacy `Tokenics` rate/budget/cost contracts are retired in favor of `AI.Synapse`), prompt assembly (`AI.Psyche`), and durable persistence (`Host`).

## 3. Public Surface
- Provider-agnostic execution ports and multimodal models for chat, text, embeddings, audio, and image generation.
- Autonomous agent, multi-agent group, in-process workflow, transient thread store, and context-provider ports.
- Cross-module governance seams (routing policy, invocation observer, capacity lease, credential reference resolver) and guardrail detector contracts.
- Atomic tool, filter, approval, and shared tool-pipeline contracts, plus token-counting/tokenizer contracts.
- Centralized AI error/usage vocabularies (including `RetryAfterMs` and `FailureDisposition` metadata keys) and static model catalog contracts.

## 4. Defaults & Overrides
- Ships zero-dependency `NoOp` and bounded in-memory fallbacks (`Fallback` tier) for standalone testing and disabled slices.
- Downstream adapter and policy modules (`AI.MEAI`, `AI.Synapse`, `AI.AgentFramework`) deterministically replace `Fallback` registrations at the `Provided` tier.

## 5. Invariants
- **AIB-01** MUST remain 100% free of third-party AI SDKs, HTTP transport types (`HttpClient` names/handlers), and persistence packages.
- **AIB-02** MUST maintain centralized error and token-usage vocabularies as the single source of truth; MUST NOT define rate-limiting, quota-budgeting, or cost-pricing contracts parallel to `AI.Synapse`.
- **AIB-03** MUST restrict the catalog to static model facts (context/output limits, capabilities, deprecation); dynamic pricing, health, and quota state belong to `AI.Synapse`.
- **AIB-04** MUST represent model capabilities as three-state facts (`Supported` / `Unsupported` / `Unknown`) without heuristic model-name guessing; consumers MUST NOT treat `Unknown` as `Unsupported`.
- **AIB-05** MUST require all atomic tool execution paths (`AI.MEAI` and `AI.AgentFramework`) to run through the shared tool filter/approval pipeline and fail closed when approval is required without a registered handler.
- **AIB-06** MUST keep observation seams non-intrusive: post-attempt observation MUST NOT alter call outcomes, propagate exceptions, or block capacity/budget lease release, and every attempt MUST carry logical invocation and attempt identities.
- **AIB-07** MUST prevent per-call override arguments from altering feature toggles or hard security ceilings (such as tool-loop round limits).
- **AIB-08** MUST keep raw prompts, completions, and resolved secret material excluded from telemetry and error descriptions by default.

</manifest>
