---
trigger: manual
---

# VK.Blocks: Core Standards (CS)

### CS.01 — Result Pattern

- Application Layer: RETURN `Result<T>` only. NEVER return null.
- For void operations, use `Result` (non-generic) or `Result<Unit>`. NEVER return bare `void` or `Task` from Application Layer handlers.
- NEVER use `Result.Failure("raw string")`. ALWAYS use predefined `Error` constants.
- **Error Constants Hierarchy**: Define error codes using the `{ModuleName}.{Category}.{Reason}` format (e.g., `Auth.ApiKey.Invalid`). Use a global `VKCoreErrors` class for shared cross-block errors, and feature-level `VK{FeatureName}Errors.cs` classes (placed at the feature slice root) for feature-specific errors to prevent constant sprawl.
- Infrastructure Layer: exceptions ARE allowed, but MUST be caught at the boundary and mapped to `Result<T>`.
- Follow RFC 7807 for HTTP error responses.
- Result<T> MUST carry structured Error objects, never raw strings or Exception objects.

### CS.02 — Layer Dependencies

- Core/Application Layer: NO direct dependency on infrastructure libraries (EF Core / Redis / Azure SDK).
- MediatR is allowed as the ONLY orchestration mechanism in the Application Layer.
- All infrastructure concerns (DB / Cache / Messaging) MUST be abstracted behind interfaces.

### CS.03 — Async

- Use `async/await` + `CancellationToken` for ALL I/O operations.
- NO `.Result`, `.Wait()`, or blocking calls.
- Prefer `ValueTask<T>` over `Task<T>` for **internal** hot-path methods where synchronous completion is the common case (cache hits, in-memory checks). **Public API interfaces (`IVK...`) MUST use `Task<T>`** to prevent consumer-side misuse (double-await, premature access). Avoid `ValueTask` when the operation is always async or may be awaited multiple times.
- ALL `await` calls within BuildingBlock/library code MUST use `.ConfigureAwait(false)` to prevent synchronization-context deadlocks.
- **Exception**: DO NOT use `.ConfigureAwait(false)` in Test methods (xUnit). Test code should maintain the synchronization context for stable assertion handling and parallelism management.
- **Enforcement**: Configure `.editorconfig` (e.g., `dotnet_diagnostic.xUnit1030.severity = error`) or use custom analyzers to automatically prevent IDE auto-completion from injecting `.ConfigureAwait(false)` into test projects.

### CS.04 — Performance

- NO database queries inside loops.
- `.AsNoTracking()` is DEFAULT for all read queries.
- Batch operations MUST use `ExecuteUpdateAsync` / `ExecuteDeleteAsync` where applicable.
- NEVER use `ToListAsync()` without explicit pagination (`Take`/`Skip`) on unbounded queries.
- Prefer projection (`Select`) over full entity materialization for read-only queries.
- Prefer `ReadOnlySpan<T>` / `Span<T>` for string parsing and manipulation to avoid heap allocations.
- Only use `stackalloc` for constant or provably small sizes (≤ 256 bytes) to prevent stack overflow risks.
- Prefer `ArrayPool<T>.Shared` for large temporary buffers (> 256 bytes) to reduce GC pressure and avoid LOH allocations. ALWAYS return the array in a `finally` block.

### CS.05 — Automation

- `IAuditable` fields (CreatedAt / UpdatedAt / CreatedBy) MUST be handled via DbContext Interceptors.
- `ISoftDelete` MUST be handled via DbContext Interceptors + Global Query Filters.
- NO manual audit or soft-delete logic in application code.

### CS.06 — Core Abstractions

- **Deterministic Logic**: PROHIBIT direct use of non-deterministic system APIs within BuildingBlocks.
- **GUIDs**: Use `IVKGuidGenerator` (injected) instead of `Guid.NewGuid()`.
- **Time**: Use `TimeProvider` (injected) instead of `DateTime.UtcNow` or `DateTimeOffset.Now`.
- **Serialization**: Use `IVKJsonSerializer` (injected) for all JSON operations to ensure consistent behavior and standard options.

### CS.07 — Dependency Resolution

- **Mandatory Registration**: ALWAYS use `GetRequiredService<T>()` or `GetRequiredService(Type)` when retrieving services from `IServiceProvider` that are expected to be registered.
- **Prohibit GetService**: The use of `GetService<T>()` or `GetService(Type)` is STRICTLY PROHIBITED for required dependencies, as it returns `null` and violates the deterministic failure principle.
- **Optional Services**: For truly optional services, use `GetServices<T>()` and check for an empty collection, or explicitly document why `GetService` is used with a null-coalescing fallback (e.g., `sp.GetService<T>() ?? Default`).
- **Fail Fast**: Prefer container-level failure over manual null checks in application logic.

### CS.08 — Persistence & Database Standards

- **Tenant Isolation**:
  - `TenantId` in DB MUST be `NOT NULL` (enforce via DbContext conventions/configurations).
  - Multi-tenant high-frequency query indexes MUST use `TenantId` as the leading column (`IX_{Table}_TenantId_{BizKey}`).
  - **Exception — Normalized Child Tables**: Pure child/junction tables (e.g., composite PK via parent FK + value column) that are accessed exclusively through a parent entity's navigation property are exempt from `IVKTenantScoped`. Tenant isolation is implicitly guaranteed by the parent's EF Core Global Filter. Adding `TenantId` to such tables would introduce redundant data violating normalization principles.
- **Primary Key**:
  - All entities MUST have an explicit Primary Key `Id`. Use Sequential GUID / UUIDv7 to prevent B-Tree page splits.
- **Timestamps & Audit Lifecycle**:
  - `CreatedAt`: `NOT NULL` (`DateTimeOffset`), automated via interceptors.
  - `UpdatedAt`: `NULL` on creation (`DateTimeOffset`), updated only on actual modification.
  - No manual timestamp assignment in business logic.
- **String Boundaries**:
  - ALL string properties MUST specify `.HasMaxLength()` explicitly. Prohibit unbounded implicit `TEXT` / `VARCHAR(MAX)`.
- **Relationships & Cascades**:
  - ALL Foreign Keys MUST specify an explicit `.HasConstraintName("FK_{Source}_{Target}_{Prop}")`.
  - `DeleteBehavior.Restrict` or `DeleteBehavior.NoAction` is MANDATORY for business entities. Implicit `Cascade` is PROHIBITED.
- **Index Definitions**:
  - ALL Indexes MUST specify an explicit `.HasDatabaseName("IX_{Table}_{Columns}")`.
  - Unique Indexes on Soft-Delete tables MUST specify `.HasFilter("IsDeleted = 0")`.
- **Migrations & Delivery**:
  - EF Core Migrations are owned EXCLUSIVELY by the Application Host (BuildingBlocks only provide models/conventions).
  - Production deployment scripts MUST be generated using `dotnet ef migrations script --idempotent`.
