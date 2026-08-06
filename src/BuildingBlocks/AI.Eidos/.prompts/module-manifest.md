---
layer: 3
id: ai-eidos-manifest
scope: building-blocks/ai-eidos
requires: CS.01, CS.03, CS.04, CS.06, AP.01, AP.02, AP.05, BB.01, BB.02, BB.03, BB.05, BB.06
---

# AI.Eidos Manifest (Response Contract, Expression Negotiation & Output Parsing)

Governs expected output shapes, how output expectations are negotiated and expressed to LLM providers, and how raw provider responses are extracted, validated, repaired, and bound back into strongly-typed objects. 

Does not assemble prompt text/messages, does not implement provider-specific schema translations, and does not execute domain event dispatching.

##  Architectural Boundaries

| Boundary / Constraint | Rule |
| :--- | :--- |
| Depend upward only (`AI ← Prompt Assembly ← Response Contract`); lower layers must never depend on prompt assembly, and must never reference concrete provider translation assemblies. | [BB.01, R2] |
| Contract projector output must remain provider-agnostic (e.g. generic JSON Schema / provider-neutral tool descriptors). Provider-specific schema translation is forbidden inside this module. | [BB.01] |
| Field-level mapping from validated DTO properties to domain events is forbidden; contract parsing responsibility ends at DTO binding. | [BB.01] |
| Storage implementations for contract definitions/registries (e.g. database persistence) must live in the host App/Infrastructure layer, never inside this module. | [R2] |

## Contract Definition & Versioning

| Boundary / Constraint | Rule |
| :--- | :--- |
| Resolve contract definitions via cascading override hierarchy (e.g. System Default → Scope/Tenant Override → Scenario Override) rather than strict exact-match lookup. | — |
| Treat version identity and logical contract identity as distinct; a version identifier must never be the sole primary key of a logical contract. | — |
| Route contract lookups through dedicated contract resolution interfaces that delegate cascading merge logic to the registry. | — |
| Keep schema/contract migration routines confined to administrative or offline triggers on historical data; never execute migration logic on per-turn request hot paths. | — |
| Pass scenario coordinates or explicit contract overrides via request arguments using the standard Args pattern. | [AP.05] |

## Negotiation & Fallback

| Boundary / Constraint | Rule |
| :--- | :--- |
| Execute contract negotiation pre-call using static capability metadata; never re-run capability negotiation mid-stream. | — |
| Trigger contract fallback policies strictly post-call after repair has been attempted and failed, adhering to defined priority tiers (e.g. Structured Output → Tool Call → Prompt JSON). | — |
| Keep pre-call negotiation and post-call runtime fallback strictly decoupled as separate strategies. | — |

## Parsing, Validation & Repair

| Boundary / Constraint | Rule |
| :--- | :--- |
| Execute free-text extraction only as a fallback path when primary structured expression mechanisms fail. | — |
| Attempt bounded repair mechanisms (in the same expression mode) strictly before escalating to mode-switching fallback policies. | — |
| Execute DTO binding strictly after schema validation yields a successful result; binding unvalidated output is forbidden. | — |
| Treat streaming output as provisional; mid-stream fields must complete full validation and binding before being treated as final. | — |

## Pipeline Integration & Module Structure

| Boundary / Constraint | Rule |
| :--- | :--- |
| Implement cross-cutting orchestration strictly in the module's pipeline middleware at the module root, not within individual feature slices. | [BB.02] |
| Keep feature slices (Contract, Negotiation, Parsing) mutually isolated; all cross-slice orchestration must proceed via the parent pipeline middleware. | [BB.01] |
| Register slice services using idempotent `TryAdd` methods inside feature builders, composed at the root module builder level. | [AP.02, BB.03] |
| Delegate persistent audit logs to host persistence infrastructure; never introduce local audit storage slices inside this module. | [BB.01] |
| Emit diagnostic metrics and telemetry solely via unified framework diagnostics conventions; do not create local telemetry slices. | [CS.06] |
| Mask raw output content in diagnostic logs when it may contain sensitive user or PII data. | [CS.06] |

