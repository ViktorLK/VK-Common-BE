---
trigger: model_decision
---

# VK.Blocks: Core Standards (CS)

### CS.01 — Result Pattern

- Application Layer: RETURN `Result<T>` only. NEVER return null.
- For void operations, use `Result` (non-generic) or `Result<Unit>`. NEVER return bare `void` or `Task` from Application Layer handlers.
- NEVER use `Result.Failure("raw string")`. ALWAYS use predefined `Error` constants.
- **Error Constants Hierarchy**: Define error codes using the `{ModuleName}.{Category}.{Reason}` format (e.g., `Auth.ApiKey.Invalid`). Use a global `VKCoreErrors` class for shared cross-block errors, and an `Internal/Errors.cs` class for block-specific errors to prevent constant sprawl.
- Infrastructure Layer: exceptions ARE allowed, but MUST be caught at the boundary and mapped to `Result<T>`.
- Follow RFC 7807 for HTTP error responses.
- NEVER throw exceptions across layer boundaries.
  Exceptions MUST be caught and mapped to Result<T> at the Infrastructure boundary.
- Result<T> MUST carry structured Error objects, never raw strings or Exception objects.

### CS.02 — Layer Dependencies

- Core/Application Layer: NO direct dependency on infrastructure libraries (EF Core / Redis / Azure SDK).
- MediatR is allowed as the ONLY orchestration mechanism in the Application Layer.
- All infrastructure concerns (DB / Cache / Messaging) MUST be abstracted behind interfaces.

### CS.03 — Async

- Use `async/await` + `CancellationToken` for ALL I/O operations.
- NO `.Result`, `.Wait()`, or blocking calls.
- Prefer `ValueTask<T>` over `Task<T>` for interfaces and hot-path methods where synchronous completion is the common case (cache hits, in-memory checks). Avoid `ValueTask` when the operation is always async or may be awaited multiple times.
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
