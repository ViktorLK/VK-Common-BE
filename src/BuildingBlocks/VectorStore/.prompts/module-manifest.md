<manifest id="vectorstore-manifest" layer="Tier 2 — Domain Capability" scope="building-blocks/vectorstore" requires="CS.01, CS.03, CS.04, AP.01, AP.04, OR.02">

# VK.Blocks VectorStore Manifest (Layer 3)

Thin governance facade built directly on `Microsoft.Extensions.VectorData`. Provides unified VK governance (VKResult, strict tenant isolation, logical alias routing, startup dimension validation, and telemetry) without maintaining speculative database abstractions. Persistence and connectors are delegated to official vector data providers (SqliteVec, Qdrant, pgvector, InMemory).

## Architectural Boundaries

### 1. Thin Governance Facade
- Wraps `Microsoft.Extensions.VectorData.VectorStoreCollection<TKey, TRecord>` via a single thin facade (`VKVectorCollectionFacade<TRecord>`).
- Does NOT duplicate low-level CRUD or capability models already provided by official VectorData.
- Enforces `VKResult<T>` failure semantics [CS.01] across all boundaries.

### 2. Tenant Isolation & Security
- `TenantId` is strictly injected into query filter expressions and verified on stored records [OR.02].
- Tenant filtering is applied at the query level against the underlying provider; cross-tenant data leakage is prohibited.

### 3. Logical Collection Registry & Alias Routing
- Exposes a logical name registry mapping logical collection names to physical collections or active aliases.
- Blue-green re-embedding and index migrations switch aliases deterministically without application restart.
- Validates configured `EmbeddingModelId` and vector `Dimensions` at application startup [BASE-03].

### 4. Direct Embeddings Generation Integration
- Supports pairing collections directly with `IEmbeddingGenerator` via official VectorData conventions, while allowing explicit vector queries.

### 5. Capability Resolution via Runtime Reflection
- Optional capabilities (such as hybrid keyword search) are determined at runtime by checking standard interfaces (`is IKeywordHybridSearchable<TRecord>`), not static capability flags.

</manifest>
