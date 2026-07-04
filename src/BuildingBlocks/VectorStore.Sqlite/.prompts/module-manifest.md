---
layer: 3
id: vectorstore-sqlite-manifest
scope: building-blocks/vectorstore-sqlite
requires: CS.01, CS.03, CS.04, AP.01, OR.01, OR.03
---

# VK.Blocks VectorStore.Sqlite Manifest (Layer 3)

SQLite provider implementation of `IVKVectorStore` using the `sqlite-vec` native extension. Dual-table schema (metadata + vec0 virtual table) joined by ROWID.

## Architectural Boundaries

### 1. Dual-Table Schema
- Metadata table (standard SQLite) + Vector table (`vec0` virtual table), correlated by ROWID.
- The ROWID join invariant MUST be maintained during all CRUD operations.

### 2. Native Extension Loading
- Platform-aware resolution (OS + architecture). Probe order: RID-specific path → base directory → bare filename. This order MUST NOT change.
- Extension load failures MUST be logged but MUST NOT throw — graceful degradation allows metadata-only operations.

### 3. Lazy Collection Init
- Per-collection table creation is lazy on first access with thread-safe double-check pattern.
- Schema is additive only (`CREATE ... IF NOT EXISTS`). No migration framework.

### 4. Transactional CRUD
- Upsert and Delete MUST execute across both tables within a single transaction. Failure = rollback.
- Delete of non-existent ID = no-op (idempotent). Search is read-only (no transaction).

### 5. Score Normalization
- `sqlite-vec` returns cosine distance (0.0–2.0). Implementations MUST convert to similarity and filter by minimum score threshold.

### 6. Vector Binary Format
- Vectors MUST be converted to raw `float32` bytes via `MemoryMarshal`. No JSON array or string serialization — raw bytes only.

### 7. Resilience
- All operations MUST be wrapped in a named Polly resilience pipeline.

