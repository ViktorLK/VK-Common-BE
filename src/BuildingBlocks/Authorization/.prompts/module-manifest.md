---
layer: 3
id: authz-manifest
scope: building-blocks/authorization
requires: CS.02, CS.06, AP.04, BB.07
---

# VK.Blocks Authorization Manifest (Layer 3)

Handles "what you can do" — permission definition, policy evaluation, and resource-level access control. Strictly excludes identity verification (Authentication's exclusive domain).

## Architectural Boundaries

### 1. Definition-First Permission Source

- All permissions MUST originate from code-level declarations via `[GeneratePermissions]`. The Source Generator MUST produce strongly-typed permission attributes and a single `PermissionsCatalog.All` registry. Hand-maintained permission strings/constants outside this pipeline are PROHIBITED.

### 2. Code-to-DB Sync Ownership

- Synchronization between code-defined permissions and the persistence layer MUST flow through `IPermissionStore` + the shared `IVKSyncStateStore` pattern (Core). Authorization MUST NOT implement a bespoke sync mechanism independent of this shared contract.

### 3. Multi-Tenant Isolation

- Permissions, roles, and policy assignments MUST be scoped by `TenantId`. Cross-tenant permission resolution or caching without tenant partitioning is PROHIBITED.

### 4. Declarative Composition

- Permission checks MUST support both declarative (`RequireAllPermissions` / `RequireAnyPermission` attributes) and imperative (in-handler) evaluation via the same underlying evaluator — no duplicated logic paths.

### 5. Fail-Closed by Default

- Any failure, timeout, or unavailability in the authorization evaluation path MUST default to deny (Fail-Closed). Fail-Open behavior is permitted only via explicit, documented allowlist configuration and requires an ADR.

### 6. Performance on Hot Path

- Permission evaluation on request-critical paths MUST be O(1)/O(log N) — backed by cache, not per-request database queries. Cache invalidation MUST be triggered synchronously on permission/role mutation.

### 7. Resource-Level Authorization

- Authorization MUST support instance-level checks (e.g. "can edit _this_ order"), not only action-level checks. Resource-based evaluators MUST be pluggable per aggregate/entity without modifying the core evaluation pipeline.

### 8. Strict Authentication Decoupling

- Authorization MUST consume only the transformed Claims/Identity produced by Authentication (`VKClaimTypes`, `IVKAuthenticatedUserContext`). It MUST NOT perform token validation, IDP calls, or identity resolution of any kind.

### 9. Audit on Denial

- Authorization denials on sensitive operations MUST be optionally auditable (who/what/when/result) via a pluggable audit hook, without forcing audit logging as a hard dependency for all evaluations.

### 10. Test-Friendly Evaluation Context

- The evaluator MUST be mockable to simulate "execute as user with permission set X" in unit tests, without requiring a real Claims/Token pipeline.
