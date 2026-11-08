<manifest id="vectorsearch-manifest" layer="Tier 2 — Domain Capability" scope="building-blocks/vectorsearch" requires="CS.01, CS.03, CS.04, AP.01, OR.03">

# VK.Blocks VectorSearch Manifest (Layer 3)

Focused retrieval enhancement layer providing hybrid Reciprocal Rank Fusion (RRF), Rerank ports, and relational join integration on top of `VectorStore`. Heavy, speculative, or overlapping stages (Compression, unproven query rewriting, semantic caching) are excised or demoted to optional extensions.

## Architectural Boundaries

### 1. Minimalistic Core Retrieval Scope
- Owns RRF score fusion when underlying stores lack native hybrid search, or when merging hits across multiple collections.
- Owns a lightweight port for Cross-Encoder / Cohere reranking.
- Owns relational join helpers to correlate vector hits back to domain entities.

### 2. No Duplicate Storage or CRUD Contracts
- Prohibits duplicate storage abstractions like `IVKRetrievalStore`. All operations delegate directly to `VectorStore` facades.
- Paging and filtering use official VectorData options (`Top`, `Skip`, filter expressions).

### 3. Context & Budgeting Boundary
- Prompt compression and token budget truncations are NOT owned here. They are delegated to domain orchestration (`Corpus`, `Weaving`, `Engram`).

### 4. Optional / Experimental Features
- Semantic caching, HyDE, and query expansion remain optional off-by-default extensions, enabled only when empirical retrieval evaluation proves net benefit.

</manifest>
