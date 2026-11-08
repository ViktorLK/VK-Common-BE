<manifest id="vectoringest-manifest" layer="Tier 2 — Domain Capability" scope="building-blocks/vectoringest" requires="CS.01, CS.03, CS.04, AP.01, OR.02, OR.03">

# VK.Blocks VectorIngest Manifest (Layer 3)

Document ingestion and preprocessing pipeline leveraging `Microsoft.Extensions.DataIngestion` while enforcing VK enterprise governance, deterministic idempotency, and specialized chunking models.

## Architectural Boundaries

### 1. Underlying Pipeline Engine
- Delegates document loading, standard token/section chunking, and basic ingestion pipeline execution to `Microsoft.Extensions.DataIngestion`.
- Isolates third-party document readers (PDF, Word, OCR) into an external adapter package per ARCH-04.

### 2. VK Governance & Storage Routing
- Exposes a thin pipeline runner wrapping `IngestionPipeline<T>` with `VKResult` error semantics, task/job tracking, and tenant context.
- Uses a custom `VKVectorStoreWriter` routing writes strictly through `VectorStore` governance facades rather than raw direct database writers.

### 3. Deterministic Chunking & Version Tracking
- Chunk IDs are generated deterministically via `Hash(TenantId + DocumentId + ContentHash)` for natural idempotency and deduplication.
- St लॉग्స్ (logs) and stamps `EmbeddingModelId` and model version on each chunk to enable automatic detection of outdated embeddings during re-indexing.
- Implements specialized `HierarchicalChunker` (Parent-Child) and `RecursiveChunker` with CJK-aware token boundaries.

</manifest>
