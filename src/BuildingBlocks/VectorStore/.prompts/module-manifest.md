---
layer: 3
id: vectorstore-manifest
scope: building-blocks/vectorstore
requires: CS.01, CS.03, CS.04, AP.01, AP.04, OR.02
---

# VK.Blocks VectorStore Manifest (Layer 3)

Pure abstraction layer for vector storage and retrieval. Defines the unified API surface and domain models without binding to any concrete database. Persistence is delegated to provider packages (Sqlite, Cosmos, etc.).

## Architectural Boundaries

### 1. Strict Abstraction Boundary
- This package MUST depend only on `VK.Blocks.Core`. No database client libraries permitted.
- All concrete implementations MUST reside in separate provider packages. Only InMemory is built-in (zero-infrastructure default, not for production).

### 2. Collection-Centric Design
- The store exposes named, typed collections. All CRUD and search operations are scoped to a collection.
- Vectors are NEVER stored without their associated document.

### 3. Capability Interfaces (Progressive Enhancement)
- Base interface = minimum contract (CRUD + similarity search). Optional capabilities (Bulk, Hybrid) are additive — callers MUST check via `is` pattern.
- Base methods MUST still work correctly on providers that also implement capability interfaces.

### 4. Tenant Isolation
- `TenantId` is `required` on both search args and stored metadata. Every operation MUST be tenant-scoped.
- Provider implementations MUST enforce tenant filtering at the query level, not application-level post-filter.

### 5. Vector Model
- Vectors use `ReadOnlyMemory<float>` (zero-copy). Implementations MUST NOT copy vector data unnecessarily.
- Provider implementations MUST normalize similarity scores to 0.0–1.0 range regardless of underlying distance metric.

### 6. Embeddings Separation
- Embedding generation (text → vector) and vector storage (vector → persist/search) are separate concerns and MUST NOT be coupled. The calling layer orchestrates both.

### 7. Provider Registration
- Single active provider — last registration wins. Provider packages expose their own strongly-typed extension methods.

