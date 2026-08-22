---
layer: 3
id: storage-manifest
scope: building-blocks/storage
requires: CS.02, CS.06, AP.04, BB.07, OR.03
---

# VK.Blocks Storage Manifest (Layer 3)

Defines the provider-agnostic contract for blob/file storage — upload, download, metadata, access control, and lifecycle. This package MUST contain zero references to any concrete storage SDK (Azure.Storage.Blobs, AWSSDK.S3, local filesystem APIs); concrete providers are exclusively downstream packages (e.g. `Storage.AzureBlob`, `Storage.S3`).

## Architectural Boundaries

### 1. Zero Provider Dependency

- This package MUST NOT reference any concrete storage SDK. All contracts (`IVKBlobStorage`, `IVKStorageContainer`) MUST be resolvable purely against BCL + Core.

### 2. Stream-First I/O

- Upload/download operations MUST be `Stream`-based (or `IAsyncEnumerable<byte[]>` for chunked scenarios). Full-buffer (`byte[]`) APIs that force entire files into memory are PROHIBITED as the primary contract surface — permitted only as a convenience overload for small, explicitly bounded payloads.

### 3. Single Path Authority

- Storage paths/keys (`{TenantId}/{Category}/{FileId}` or equivalent) MUST be generated exclusively through a single shared Path Builder. Ad-hoc string concatenation of paths anywhere outside this builder is PROHIBITED, as it is a direct cross-tenant access risk.

### 4. Standardized Error Surface

- All storage failures (not found, quota exceeded, access denied, timeout, conflict) MUST surface through `StorageErrors` constants and the `VKResult` pattern (R1). Raw SDK exceptions MUST NOT leak past the concrete provider package.

### 5. Metadata Contract Uniformity

- `BlobMetadata` (ContentType, Size, ETag, LastModified, custom key-value) MUST be the sole metadata shape exposed to consumers, regardless of backend. Provider-specific metadata fields MUST be mapped into this contract, never exposed directly.

### 6. Conditional Write Safety

- Overwrite/concurrent-write scenarios MUST support ETag/If-Match conditional semantics. Unconditional overwrite MUST be an explicit opt-in, never the default behavior of a write operation.

### 7. Presigned URL Minimal Exposure

- Presigned URL / SAS generation MUST default to the shortest viable expiry and narrowest permission scope (e.g. read-only, single-object). Longer expiry or broader scope (write/delete, container-level) MUST be an explicit, individually justified parameter — never a default.

### 8. Multi-Tenant Isolation

- Every storage operation MUST resolve its target path/container through tenant-aware path generation (see Rule 3). Cross-tenant reads/writes MUST be structurally impossible, not merely convention-enforced.

### 9. Delegated Content Safety Validation

- File type/content safety checks (magic-number sniffing, extension allowlists, size limits) MUST be delegated to the Validation module's file-safety extension point. Storage MUST NOT implement a parallel, independent validation mechanism.

### 10. Lifecycle as Declarative Configuration

- Archival/cold-tier migration and TTL-based expiry MUST be expressed as declarative policy configuration (per container/category), not imperative per-file scheduling logic scattered across call sites.

### 11. Modular DI Registration

- `VKStorageBlock` MUST follow the standard 8-step DI registration order (R13). `IVKStorageBuilder` is the sole extension point for concrete providers — providers MUST NOT bypass it via direct `IServiceCollection` manipulation.

### 12. Resiliency for Transient Failures

- All provider-bound calls MUST be wrapped in a resilience pipeline (retry + circuit-breaker, OR.03) at the concrete-provider level, coordinated with — not duplicated against — any caller-level retry logic.

### 13. Provider-Agnostic Diagnostics

- Upload/download duration, throughput, and failure rate MUST be emitted under the shared Storage diagnostics namespace. Concrete providers MUST NOT define a competing namespace.

### 14. Test-Friendly Fake Implementation

- An in-memory or local-disk fake implementation of `IVKBlobStorage` MUST be available for unit testing without requiring a real cloud storage account.
