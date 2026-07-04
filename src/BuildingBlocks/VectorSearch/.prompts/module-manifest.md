---
layer: 3
id: vectorsearch-manifest
scope: building-blocks/vectorsearch
requires: CS.01, CS.03, CS.04, AP.01, OR.03
---

# VK.Blocks VectorSearch Manifest (Layer 3)

High-level retrieval pipeline built on the VectorStore abstraction — coordinates Query Rewrite, Hybrid Search, Semantic Cache, Reranking, and Score Fusion.

## Architectural Boundaries

### 1. Pipeline Flow
- Before Stages (Query Rewrite, Cache, Expansion) → Terminal Execution (search strategy) → After Stages (Rerank, Fusion, Compression).
- Semantic Cache failures MUST fail-open — fallback to actual retrieval, never fail the pipeline.

### 2. Hybrid Retrieval
- If the store supports native hybrid search, delegate to it. Otherwise, execute Dense + Sparse in parallel and merge locally.
- Results merging MUST support both Reciprocal Rank Fusion (RRF) and Weighted Score Fusion.

### 3. Abstraction Over Store
- MUST NOT query databases directly. All retrievals go through `IVKVectorStore` / `IVKRetrievalStore`.
- Embedding generation MUST be coordinated via `IVKEmbeddingsEngine` — no implicit embedding inside store operations.

### 4. Safe Fallback
- If LLM-based Query Rewrite or Reranking fails or times out, MUST fall back to original query and vector distance scores. Retrieval MUST NOT fail due to post-processing helper failures.

