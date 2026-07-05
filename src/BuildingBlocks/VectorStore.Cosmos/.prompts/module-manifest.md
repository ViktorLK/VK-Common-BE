---
layer: 3
id: vectorstore-cosmos-manifest
scope: building-blocks/vectorstore-cosmos
requires: CS.01, CS.03, CS.04, AP.01, OR.01, OR.03
---

# VK.Blocks VectorStore.Cosmos Manifest (Layer 3)

Azure Cosmos DB provider implementation of `IVKVectorStore` using native NoSQL Vector Search. Single container partitioned by collection name with auto-provisioning.

## Architectural Boundaries

### 1. Single-Container / Multi-Collection
- All collections share one Cosmos DB container. Collection name = partition key. Cross-partition queries are PROHIBITED.

### 2. Container Auto-Provisioning
- Database and container creation uses lazy init with double-check locking (one-time per store lifetime).
- Container MUST be created with a Vector Embedding Policy and vector index. Changing index type requires ADR.

### 3. Score Normalization
- Cosmos returns cosine distance (0.0–2.0). Implementations MUST convert to similarity and filter by minimum score threshold.

### 4. Idempotent Operations
- Upsert = native create-or-replace. Delete of non-existent document = no-op (`VKResult.Success`). GetById miss = `Success(null)`, not failure.

### 5. Serialization
- Cosmos SDK v3 uses `Newtonsoft.Json`. Internal DTOs MUST use Newtonsoft attributes, NOT `System.Text.Json`. Vectors stored as `float[]` at the provider boundary.

### 6. Resilience
- All operations MUST be wrapped in a named Polly resilience pipeline. Connection strings MUST NOT be logged.

