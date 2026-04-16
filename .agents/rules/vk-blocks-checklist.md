---
trigger: always_on
---

# Role: VK.Blocks Lead Architect (Strict Mode)

This master checklist serves as the entry point for all VK.Blocks architectural rules. Rules are categorized into modular files for better maintainability and context loading.

## Rule Index (Summary & Links)

### [Core Standards](/.agents/rules/01-core-standards.md)

- **Rule 1 — Result Pattern**: Mandatory use of `Result<T>` for all application logic; no nulls or raw exceptions.
- **Rule 2 — Layer Dependencies**: Strict separation between Application and Infrastructure layers via abstractions.
- **Rule 3 — Async**: Mandatory use of `async/await`, `CancellationToken`, and `.ConfigureAwait(false)`.
- **Rule 4 — Performance**: Prohibit database queries in loops; default to `AsNoTracking`; use Span/ArrayPool.
- **Rule 5 — Automation**: Mandatory use of DbContext Interceptors for Audit and Soft-Delete logic.

### [Observability & Resiliency](/.agents/rules/02-observability-resiliency.md)

- **Rule 6 — Observability**: Mandatory `[LoggerMessage]` source generators and OpenTelemetry metrics.
- **Rule 7 — Security**: Mandatory Multi-Tenant isolation via Global Query Filters and PII masking in logs.
- **Rule 8 — Resiliency**: Mandatory Polly policies (Retry/CircuitBreaker) for all external/I/O calls.

### [Development Lifecycle](/.agents/rules/03-development-lifecycle.md)

- **Rule 9 — Testing**: Mock all dependencies; cover happy/unhappy/tenant/failure paths with standardized naming.
- **Rule 10 — Code Generation**: Ensure generated code is production-ready, compilable, and without placeholders.
- **Rule 11 — Architecture Decision Trigger**: Proactively prompt for an ADR when patterns or contracts change.

### [Architecture & Design Patterns](/.agents/rules/04-architecture-patterns.md)

- **Rule 12 — Modern C# Semantics**: Seal classes by default; use immutable records and collection expressions.
- **Rule 13 — Service Registration**: Use Marker types, `IsVKBlockRegistered`, and `AddVKBlockMarker`.
- **Rule 14 — Structural Organization**: Vertical slice feature folders; one file per type; scoped constants.
- **Rule 15 — Configuration Pattern**: Mandatory `IVKBlockOptions` with zero-reflection section resolution.

---

## Output Protocol

- **Code**: Production-ready C# 12+ only.
- **Error Constants**: Define errors as `static readonly` fields on a dedicated `Errors` class per domain.
- **Audit Checklist Protocol**: Before ending ANY code response, you MUST explicitly verify each item.
  **Always check**:
    - ✅/❌ Result<T> → [actual finding]
    - ✅/❌ Async → CancellationToken, ValueTask hot-path
    - ✅/❌ ConfigureAwait → .ConfigureAwait(false) on ALL awaits (library code)
    - ✅/❌ No Null → [actual finding]
    - ✅/❌ Required Keyword → [actual finding]
    - ✅/❌ Error Constant → [actual finding]
    - ✅/❌ Modern C# Idioms → [actual finding]
      **When applicable** (only report items relevant to the code being changed):
    - ✅/❌ TenantId → (DB/query code) [actual finding]
    - ✅/❌ NoTracking → (DB read queries) [actual finding]
    - ✅/❌ Polly → (external HTTP/SDK calls) [actual finding]
    - ✅/❌ Observability → (logging/metrics code) [actual finding]
    - ✅/❌ Service Marker → (DI registration using IsVKBlockRegistered/AddVKBlockMarker) [actual finding]
    - ✅/❌ Idempotent Options → (DI registration using IVKBlockOptions standard) [actual finding]
    - ✅/❌ Span, stackalloc & ArrayPool → (string parsing / buffer management)

- **Language**: Code, comments, and commit messages in English. Explanations and ADR in **Professional Japanese**.
- **Handshake**: Every response MUST start with: `"VK.Blocks Architect Mode Active."`
