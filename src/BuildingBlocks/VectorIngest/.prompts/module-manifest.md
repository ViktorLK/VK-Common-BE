---
layer: 3
id: vectoringest-manifest
scope: building-blocks/vectoringest
requires: CS.01, CS.03, CS.04, AP.01, OR.02, OR.03
---

# VK.Blocks VectorIngest Manifest (Layer 3)

Document ingestion and preprocessing pipeline — parses raw documents, chunks text, enriches with metadata, deduplicates, generates embeddings, and indexes into a VectorStore sink.

## Architectural Boundaries

### 1. Pipeline Lifecycle
- Ingestion flows sequentially: Loading → Parsing → Chunking → Enrichment → Deduplication → Embedding → Indexing.
- Stages execute in fixed order through a shared context.

### 2. Chunking Safety
- Chunk sizes MUST NOT exceed downstream embedding model token budgets.
- Text splitting MUST respect token boundaries — no mid-word or mid-sentence breaks.

### 3. Tenant Isolation
- All ingestion requests MUST carry a `TenantId`. The Enrichment stage MUST stamp it on every chunk before writing to the vector store.

### 4. Decoupled Persistence
- The pipeline MUST interact with storage purely through `IVKVectorStore` / `IVKVectorCollection<T>`. Direct database client instantiation is PROHIBITED.

