---
layer: 3
id: ai-engram-manifest
scope: building-blocks/ai-engram
requires: CS.01, CS.03, CS.04, CS.06, AP.01, AP.02, BB.01, BB.02, BB.03, BB.05
---

# AI.Engram Manifest (L3 — Cognitive Memory Lifecycle & Retention Governance)

Manages Compression, Consolidation, Decay, Pruning, and Contradiction Arbitration of AI conversation memories.

## Architectural Boundaries

| Boundary / Constraint | Rule |
| :--- | :--- |
| Never couple directly to specific Vector DBs; access long-term storage via `IVKMemoryEchoes` / `VectorSearch` APIs. | [BB.01] |
| Separate objective facts (Core shared, `PersonaId = null`) from persona relationship memories (`PersonaId` scoped). | — |
| Keep structured KV facts (`IVKMemoryStructured`) isolated from vector Decay & Pruning score pipelines. | — |

## Memory Tier Transitions & Compression

| Boundary / Constraint | Rule |
| :--- | :--- |
| Trigger L1 (Echo) → L2 (Summary) compression via turn threshold or timer asynchronously. | [CS.03] |
| Execute L2 → L3 consolidation during `AfterStage` or background execution. | [CS.03] |
| Guarantee consolidation idempotency using marker records within `VKPsycheContext.SetState`. | [AP.01] |
| Enforce length (&gt;2000 chars) and prompt injection keyword checks before persisting memory. | [AP.01] |

## Decay, Pruning & Revision

| Boundary / Constraint | Rule |
| :--- | :--- |
| Enforce Ebbinghaus forgetting curve with access-frequency adjusted half-life for `RetentionScore`. | — |
| Respect `IsPinned` flag by skipping pruning and decay processing completely. | — |
| Force prune entries exceeding `HardTtl` regardless of `RetentionScore`. | — |
| Execute similarity deduplication at the end of consolidation/pruning using Cosine Similarity. | [CS.04] |
| Arbitrate fact contradictions using `IVKContradictionArbitrator` and mark overruled memories as `IsSuperseded = true`. | [CS.01] |

## Registration & Infrastructure Isolation

| Boundary / Constraint | Rule |
| :--- | :--- |
| Register all block services via idempotent `TryAdd` methods in `VKAIEngramBlock`. | [AP.02, BB.03] |
| Isolate all background processors (Decay, Pruning, Compression, Reminders) from client pipeline exceptions. | [CS.03] |
| Use `IVKDistributedLockProvider` for background job coordination across multiple replicas. | [CS.03] |
| Mask PII content (SHA256 log hashing) in diagnostic logging and telemetry. | [CS.06] |
| Delegate DLQ fallback to `IVKConsolidationDlqHandler` or logging without direct `Messaging` package coupling. | [BB.01] |
