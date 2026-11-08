<manifest id="ai-engram" layer="Tier 2" scope="src/BuildingBlocks/AI.Engram" requires="CS.01, CS.03, CS.04, CS.06, OR.01, BB.04, BB.06">

# 📦 Module Manifest: AI.Engram

## 1. Layer & Purpose (ARCH-01)
- **Layer**: Tier 2 — Domain Capability (`VK.Blocks.AI.Engram`).
- **Purpose**: Owns 3-layer cognitive context engineering & memory lifecycle:
  - **L0 Episode Log (`Episodes/`)**: Immutable, append-only conversational experiences (`IVKEpisodeRepository`, `VKEpisode`) serving as single source of truth.
  - **L1 Derived Memory (`MemorySemantic/`, `MemoryStructured/`, `MemorySummary/`)**: Bi-temporal validity (`ValidFrom`/`ValidTo`), entity links, and L0 provenance tracking (`SourceEpisodeIds`). Outbound reminder dispatch is delegated to `IVKReminderWorkflowPort`.
  - **L2 Recall & Presentation (`Recall/`)**: Hybrid search over `VK.Blocks.VectorSearch`, side-effect-free cognitive decay (`IVKRecallDecayStrategy`), and human-like presentation policy (`IVKRecallPresentationPolicy`).
  - **Governance (`Reclamation/`)**: Distinguishes index-cost archival/pruning from GDPR/APPI cascade physical deletion (`IVKMemoryErasureService`).

## 2. Dependency Boundaries (ARCH-01, ARCH-02)
- **Allowed Dependencies**:
  - `VK.Blocks.Core` (Primitives, `Result<T>`, `TimeProvider`, `IVKGuidGenerator`)
  - `VK.Blocks.AI` / `VK.Blocks.AI.Psyche` / `VK.Blocks.AI.Eidos` (Dialogue, prompt context, and schema-driven extraction)
  - `VK.Blocks.VectorStore` / `VK.Blocks.VectorSearch` (Vector persistence and hybrid retrieval)
- **Forbidden Dependencies**:
  - Direct third-party Vector DB drivers (Qdrant, Milvus SDKs) or EF Core in domain layers.
  - Ownership of background scheduling or job workers for external workflows (reminders delegated via port).

## 3. Public Surface & Extension Contracts (ARCH-07, BASE-12)
- **L0 Invariant**: `VKEpisode` is strictly append-only and immutable.
- **L1 Invariant**: All derived memories implement `IVKDerivedMemory` exposing `SourceEpisodeIds`. State updates close `ValidTo` non-destructively.
- **L2 Invariant**: Recall operates within `TokenBudget` without mutating L1 importance or database storage. Presentation applies deterministic hedging without additional LLM round-trips.
- **Reclamation Invariant**: Routine background pruning never permanently deletes database entities (`ADR-002`); physical erasure is executed strictly via `IVKMemoryErasureService` for GDPR/APPI compliance and hard TTL expiration.

## 4. Defaults & Overrides (ARCH-05, AP.02)
- **Co-Located Defaults**:
  - In-memory repositories (`InMemoryEpisodeRepository`, `InMemoryMemorySemanticRepository`, `InMemoryMemoryStructuredRepository`, `InMemoryMemorySummaryRepository`, `InMemoryCompressionJobStore`) registered via `TryAdd` in vertical slice features.
  - Default policies (`HumanLikePresentationPolicy`, `ExponentialRecallDecayStrategy`, `DefaultMemoryRecallService`, `DefaultMemoryErasureService`).
- **Override Mechanism**: Downstream hosts replace repositories, compression strategies, or presentation policies via standard DI builder registration.

## 5. Domain Invariants (BASE-01)
- **Scope Separation**: Multi-tenant isolation (`TenantId NOT NULL`) is strictly enforced across L0, L1, and Vector collections.
- **Single-Pass Extraction**: Fact extraction, categorization, scoring, and arbitration are coordinated in a single Eidos structured JSON call.
- **Telemetry & Privacy**: PII is scrubbed before emitting OpenTelemetry tags or structured logs (`CS.06`, `OR.01`).

</manifest>
