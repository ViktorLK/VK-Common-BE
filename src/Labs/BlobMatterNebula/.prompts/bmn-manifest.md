---
layer: 3
id: bmn-manifest
scope: module
extends: vk-research-manifesto
# [BMN Specific Requirements] Supplementing the Research Manifesto
requires: CS.01
requires: CS.03
requires: CS.06
requires: AP.01
---

# BMN Rule Set: BlobMatterNebula

## BMN01: Deterministic Path Generation [uses: CS.06]

Storage paths for blobs MUST be generated using `IVKGuidGenerator` or a deterministic hashing strategy based on the `TenantId` and `FileId`. Direct string concatenation of unsanitized user input into paths is strictly prohibited to prevent path traversal and collision.

## BMN02: MIME Type & Content Validation [standalone]

Every uploaded blob MUST have its `ContentType` (MIME type) validated against an allowlist. The system MUST NOT rely solely on file extensions. In high-security research scenarios, magic-byte inspection is preferred over header-based content type reporting.

## BMN03: Tenant-Level Storage Partitioning [uses: OR.02]

Physical storage containers or logical prefixes MUST include the `TenantId`. This ensures that even at the infrastructure provider level (Azure/AWS), data from different tenants is isolated. The `FileMetadata` aggregate MUST enforce this boundary.

## BMN04: SAS Token Expiry Discipline [uses: CS.01]

All Shared Access Signature (SAS) tokens generated via `GenerateSasTokenAsync` MUST have a strictly bounded Time-To-Live (TTL). The default research TTL is 1 hour. Requests for longer-lived tokens MUST be audited and require specific `IVKUserContext` permissions.

## BMN05: Atomic Metadata-Blob Consistency [uses: CS.01, CS.03]

Operations involving both physical blob storage and SQL-based metadata (e.g., `UploadAsync` followed by metadata save) MUST be handled as an atomic logical unit. If the metadata save fails, the physical blob MUST be scheduled for cleanup (Garbage Collection) to prevent orphaned storage matter.

## BMN06: Stream Resource Management [uses: CS.03]

All blob upload/download operations MUST support `CancellationToken` propagation to the underlying SDK. Streams MUST be disposed of correctly using `using` blocks or `IAsyncDisposable`. In-memory buffering of large blobs is prohibited; use streaming interfaces only.

## BMN07: Immutable Blob Strategy (WORM) [standalone]

To maintain research integrity, blobs in the Nebula are treated as **Immutable**. Updating a file creates a new `FileMetadata` record with a new version or unique path. Deletion is "Soft Delete" by default, where the metadata is marked, and the physical blob is moved to a "cool" archive.

## BMN08: Malware Scanning Hook [uses: CS.01]

The application layer MUST provide a hook or decorator for malware scanning. Any blob uploaded MUST be marked as `PendingScan` and only transition to `Available` once the scanning service emits a success result.

## BMN09: Boundary Size Constraints [standalone]

The system MUST enforce strict `MaxFileSize` limits at the Application service boundary. These limits should be configurable via `IVKBlockOptions` to prevent Denial-of-Service (DoS) attacks via massive storage consumption.

## BMN10: Provider Agnosticism [extends: BB.01]

Core logic in `FileService` MUST depend only on `IVKStorageProvider` (Domain Abstractions) and never on `Azure.Storage.Blobs` directly. Provider-specific features (like Azure Blob Index Tags) MUST be abstracted behind generic metadata interfaces.

---

## BMN Audit Checklist (Phase C: Domain Audit)

- [ ] **Path Determinism (BMN01)**: Paths generated using IVKGuidGenerator/TenantId prefix.
- [ ] **MIME Validation (BMN02)**: ContentType validated against allowlist.
- [ ] **Tenant Isolation (BMN03)**: Storage containers/prefixes partitioned by TenantId.
- [ ] **SAS TTL (BMN04)**: SAS tokens have strict expiry (default 1h).
- [ ] **Stream Discipline (BMN06)**: CancellationToken propagated and streams disposed.
- [ ] **Immutability (BMN07)**: Updates create new versions; no direct overwrites.
- [ ] **Size Enforcement (BMN09)**: MaxFileSize limits enforced at the boundary.
- [ ] **Abstraction Integrity (BMN10)**: No direct leak of provider SDKs in Application layer.
