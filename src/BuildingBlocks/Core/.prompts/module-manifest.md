---
layer: 3
id: core-manifest
scope: building-blocks/core
requires: CS.01, CS.02, CS.04, AP.01, AP.02, AP.04, AP.05
---

# VK.Blocks Core Manifest (Layer 3)

Defines the foundational abstractions that all other modules depend on. Represents the highest level of architectural purity. Every other Building Block (Persistence, Storage, Web, Observability, Auth, Authz, Validation, AI, ...) transitively depends on Core; Core MUST NOT depend on any of them.

## Architectural Boundaries

### 1. Abstraction Purity

- Any new contract in Core MUST be zero-dependency relative to external libraries (BCL only).
- Core MUST provide baseline implementations for JSON serialization and memory-efficient utilities.

### 2. Zero-Reflection

- Hot paths MUST NOT use runtime reflection or LINQ. Use Static Abstract Interface Members, Source Generators, `Span<T>`, and `stackalloc` instead.

### 3. Result Pattern Ownership

- `VKResult` / `VKResult<T>` is the sole return contract for the entire framework. `VKResult.Success()` is a cached singleton — do not allocate new instances.

### 4. Error Model Ownership

- `Errors/` (`VKError`, `VKErrorType`, `VKErrorDetail`, `VKErrorResponse`, `VKErrorDebugInfo`) is the single, framework-wide error data primitive. It is consumed independently by `Results/` (via `VKResult<T>.Error`), by Validation (`VKErrorDetail[]` aggregation), and by ExceptionHandling/Web (`VKErrorResponse` construction) — these are parallel consumers, not a chain through Result.
- Downstream modules (EH's `VKExceptionProblemDetails`, Web's `VKProblemDetails`) MUST compose/wrap these primitives. Defining a parallel error shape in any downstream module is PROHIBITED.
- Dependency direction is one-way: `Results/` depends on `Errors/`; `Errors/` MUST NOT reference any type in `Results/`.

### 5. DI Pipeline Ownership

- Marker pattern (`IVKBlockMarker`), idempotent registration, and recursive dependency resolution with cycle detection are Core's exclusive responsibility. Other modules MUST NOT reimplement these.
- Core is the one module exempt from the standard `VK{ModuleName}Options.cs` identity-anchor convention: Core is unconditionally loaded and has no toggleable/configurable surface. It MUST NOT introduce an `Enabled` switch.

### 6. Domain Primitives

- `VKValueObject` uses allocation-free structural equality (no LINQ). `VKEntity<TId>` uses ID-based equality. These contracts MUST NOT be overridden in downstream modules.
- `VKDomainException` intentionally lives inside `Domain/` rather than `Exceptions/` — it is domain-layer-specific and belongs to the Domain vertical slice, not the HTTP-semantic exception hierarchy in `Exceptions/`.
- Domain event dispatch timing (pre-commit collection vs. post-commit publish) is NOT decided in Core. Core only defines `IVKDomainEvent`/`IVKEventDispatcher` as protocols; the actual dispatch timing contract belongs to the Persistence layer's UnitOfWork implementation.

### 7. Tenancy as an Independent Slice

- `VKTenantId` is a `readonly record struct` with a private constructor and `VKTenantId.From()` factory — the sole strongly-typed tenant identifier for the entire framework. No module may define its own tenant identifier type.
- `Tenancy/` MUST remain a leaf slice: it MUST NOT depend on `Domain/`. `Domain/` MAY depend on `Tenancy/` (e.g. to express `IVKMultiTenantEntity`), never the reverse.

### 8. Security Slice Is Metadata-Only

- `Security/` MUST contain only PII/sensitivity markers (`VKSensitiveDataAttribute`, `VKRedactedAttribute`), metadata query protocols (`IVKSecurityMetadataProvider`, `IVKSemanticSchemeProvider`), and static policy-name constants (`VKAuthPolicies`, `VKSecurityPolicies`).
- Actual policy evaluation logic (does user X have permission Y) is PROHIBITED in Core and belongs exclusively to the Authorization module. `Security/` types describe _what a policy is called_ and _what data is sensitive_ — never _whether access is granted_.
- The PII redaction pipeline built on this metadata is the single shared redaction mechanism referenced by Observability (per `observability-manifest` Rule 7) — Core MUST NOT duplicate redaction logic elsewhere.

### 9. Filtering vs. Specification

- `Filtering/` (`IVKEntryFilter`, `VKFilterVerdict`) is a runtime, single-item accept/reject protocol for imperative pipeline scenarios (e.g. AI.Corpus). It is NOT a query-composition mechanism.
- Declarative, translatable query conditions (Specification pattern) are explicitly out of scope for Core and are owned by the consuming query/persistence library. Core MUST NOT grow a competing Specification abstraction.

### 10. Structural Convention Compliance

- Every slice (`Domain/`, `Errors/`, `Results/`, `Tenancy/`, `Security/`, `Identity/`, `Serialization/`, `Synchronization/`, `Mapping/`, `Guids/`, `Filtering/`, `Pipeline/`) MUST follow the standard `Models/` (public, `VK`-prefixed) / `Protocols/` (public, `IVK`-prefixed) / `Internal/` (non-public, unprefixed) sub-structure once a slice exceeds a single concept-file. Single-file or already-atomic slices (e.g. `Guards/`) are exempt from further subdivision.
- `NoOp`/`Null` default implementations (`VKNoOpSyncStateStore`, `NullUserContext`) are placed under `Internal/` regardless of whether they are publicly resolvable via DI — replaceable-default status does not change their structural classification.

### 11. Planning Discipline

- Architectural changes to Core MUST include a formal ADR and a full regression-test plan in the Walkthrough. This includes any change to dependency direction between slices (Rule 7), any expansion of `Security/` beyond metadata (Rule 8), and any reconsideration of Rule 9's Filtering/Specification boundary.
