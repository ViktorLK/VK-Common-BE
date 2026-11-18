<manifest requires="AP.04, AP.05, AP.06, BB.05, BB.06, BB.07, BB.08" allowed="VK.Blocks.Core, Microsoft.Extensions.DependencyInjection.Abstractions, Microsoft.Extensions.Logging.Abstractions, Microsoft.Extensions.Options" forbidden="Microsoft.Extensions.AI*, Microsoft.Agents.AI*, Microsoft.SemanticKernel*, OpenAI*, Azure*, Ollama*, Google*, Anthropic*, Mscc.*">

# VK.Blocks AI Manifest

Owns provider-agnostic AI contracts, shared error and usage vocabularies, static model catalog, atomic tool/approval/filter contracts, and token-counting contracts with default approximate implementations; delegates concrete execution, agent runtimes, and stateful policies to downstream modules.

## 1. Ownership
- **Owns**: OWN-PORT-CONTRACT, OWN-PORT-ERRORS, OWN-CATALOG, OWN-TOOL-CONTRACT, OWN-TOKEN-COUNT
- **Delegates**: OWN-MODEL-CALL → AI.MEAI, OWN-PROVIDER-TRANSLATION → AI.MEAI, OWN-TOOL-LOOP → AI.MEAI, OWN-TOOL-FILTER → AI.MEAI, OWN-AGENT-RUN → AI.AgentFramework, OWN-AGENT-GROUP → AI.AgentFramework, OWN-AGENT-WORKFLOW → AI.AgentFramework, OWN-ROUTE → AI.Synapse, OWN-QUOTA → AI.Synapse, OWN-COST → AI.Synapse

## 2. Boundaries
- **Non-goals**: Durable persistence of conversation history, audit logs, or secrets — belongs to `Host` (reconsider only if a shared storage port is needed across multiple blocks).
- Concrete model invocation, provider-specific schema translation, and stateful routing/quota evaluation are excluded here and delegated above.

## 3. Public Surface
- **Execution & Multimodal Ports**: Provider-agnostic engine contracts and multimodal message models for chat, text/chunking, embeddings, audio, and image generation.
- **Agent, Tool & Governance Seams**: Agent runtime ports, atomic tool/approval/filter contracts (execution delegated to `AI.MEAI`), static model catalog, pre-call routing/post-attempt observation ports, guardrails, and token-counting contracts with default approximate implementations.
- **Shared Primitives**: Centralized error/usage vocabularies, request-override argument base contracts, and keyed engine registration extensions.

## 4. Defaults & Overrides
- Ships `NoOp` and bounded in-process fallbacks for standalone use; multi-provider engines register via keyed DI.

## 5. Invariants
### Contracts & Catalog
- **AIB-01** MUST keep all ports, message models, and options 100% free of third-party AI SDK types.
- **AIB-02** MUST maintain centralized error and token-usage vocabularies as the single source of truth across all AI modules; downstream modules never define parallel shapes.
- **AIB-03** MUST restrict the catalog to static model facts (context limits, capabilities, deprecation status); time-effective pricing, connection health, and live quota state belong to downstream policy modules.
- **AIB-04** MUST represent model capabilities as three-state facts (supported / unsupported / unknown); consumers MUST NOT treat unknown as unsupported.

### Tools & Port Semantics
- **AIB-05** MUST require all atomic tool execution implementations to honor the shared tool filter and approval contracts; tools requiring approval MUST fail closed when no approval handler is registered.
- **AIB-06** MUST keep observation ports non-intrusive: post-attempt observation MUST NOT alter the observed call outcome or propagate exceptions to the caller, and every attempt MUST carry both a logical invocation identity and an attempt identity.

### Options & Security
- **AIB-07** MUST prevent per-call override arguments from altering feature toggles or hard security ceilings (such as tool-loop round limits).
- **AIB-08** MUST keep raw prompts and completions excluded from telemetry by default unless explicitly opted in.

</manifest>
