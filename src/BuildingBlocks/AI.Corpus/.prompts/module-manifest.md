---
layer: 3
id: ai-corpus-manifest
scope: building-blocks/ai-corpus
requires: CS.01, CS.03, CS.04, CS.06, AP.01, AP.02, BB.01, BB.02, BB.03, BB.04, BB.05, OR.01
---

# VK.Blocks AI.Corpus Manifest (L3 — 静态权威知识治理与检出/注入生命周期)

Manages Gathering, Filtering, Context Compression, Poisoning Protection, Workflow Approval, Versioning, and Usage Tracking of authoritative static knowledge entries.

## Architectural Boundaries

| Boundary / Constraint | Rule |
| :--- | :--- |
| Never couple directly to specific Vector DBs; access storage via `IVKVectorStore` / `IVKIndexingService` APIs. | [BB.01] |
| Isolate static/authoritative knowledge governance from dynamic experience memories (`AI.Engram`). | [BB.01] |
| Maintain zero coupling to user authorization logic; delegate permission checks to `Authorization` block. | [BB.01] |

## Gathering & Retrieval Pipeline

| Boundary / Constraint | Rule |
| :--- | :--- |
| Provide `InMemory` fallback stores (`InMemoryKnowledgeLifecycleStore`) for zero-infrastructure startup. | [CS.01] |
| Combine static knowledge and dynamic recall queries cleanly using `IVKRecallKnowledgeLifecycleStore`. | [CS.01, CS.03] |
| Preserve thread safety when building context candidates across stages. | [AP.01] |

## Filter Chain Ordering & Context Compression

| Boundary / Constraint | Rule |
| :--- | :--- |
| Execute filter chain in fixed priority sequence: Stickiness → Static Metadata / Language / Approval → Behavioral Gates → Mutex/Pruning → Budget/Decay. | [CS.01] |
| Honor feature toggles on `VKFilteringOptions` by bypassing disabled filters immediately. | [BB.05] |
| Reject unapproved (`Draft`, `PendingReview`, `Rejected`) knowledge entries via `ApprovalStatusFilter`. | [CS.01] |
| Gate multi-language matching via `LanguageFilter` when context and entry language codes are specified. | [CS.01] |
| Expand and compress sentence-window context around hit offset range using `IVKContextCompressor`. | [CS.01] |

## Ingestion, Poisoning Shield & Versioning

| Boundary / Constraint | Rule |
| :--- | :--- |
| Enforce length limits (`MaxContentLength`) and adversarial prompt injection pattern checks via `IVKCorpusPoisoningShield` before ingestion. | [CS.01, AP.01] |
| Persist immutable document version snapshots (`VKKnowledgeVersion`) to `IVKKnowledgeHistoryStore` on every text ingestion. | [CS.01, CS.06] |
| Perform two-phase synchronized rollback (`RollbackDocumentVersionAsync`) by clearing existing vector index and re-ingesting target historical version. | [CS.01, CS.03] |
| Define entity schema (`VKKnowledgeSchema`) for format alignment and LLM extraction prompt rendering. | [AP.01] |

## Tracking & Observability

| Boundary / Constraint | Rule |
| :--- | :--- |
| Isolate tracking stage failures (`DefaultKnowledgeInjectionStage`); catch exceptions and degrade gracefully without blocking pipeline execution. | [CS.03, OR.01] |
| Instrument OpenTelemetry metrics (`CorpusDiagnostics`) for candidate gathering, filter verdicts, injection counts, and ingestion operations. | [BB.04] |
| Use `TimeProvider` and `IVKGuidGenerator` for deterministic timestamps and identifier creation. | [CS.06] |
| Register all block dependencies idempotently using `TryAdd` / `TryAddEnumerable`. | [AP.02, BB.03] |
