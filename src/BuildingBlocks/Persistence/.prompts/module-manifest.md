---
layer: 3
id: persistence-manifest
scope: building-blocks/persistence
requires: CS.02, CS.06, AP.04, BB.07, BB.08
---

# VK.Blocks Persistence Manifest (Layer 3)

Defines ORM-agnostic contracts (repositories, unit of work, transactions, auditing). This package **MUST NOT** reference concrete data-access libraries (EF Core, Dapper, etc.); concrete implementation is deferred to provider packages (e.g., `Persistence.EFCore`).

## Core Constraints

| Area | Constraint | Associated Rule |
| :--- | :--- | :--- |
| **Dependencies** | Purely BCL + Core. Zero references to concrete ORM/DB libraries. | CS.02 |
| **Read/Write Segregation** | Keep `IVKReadRepository` and `IVKWriteRepository` separate. Read contracts must not expose write paths. Query defaults to no-tracking. | - |
| **Unit of Work** | `IVKUnitOfWork` owns the commit semantic. Repositories must not call internal SaveChanges-equivalent operations. | - |
| **Transactions** | `IVKTransaction` is strictly for multi-UnitOfWork operations or custom isolation levels. Avoid on single UnitOfWork tasks. | - |
| **Property Setter** | `IVKPropertySetter` is strictly for ORM materialization/entity hydration. Prohibited in Domain or Application layers. | BB.07 |
| **Auditing** | `IVKAuditProvider` must source time/identity exclusively from Core's `TimeProvider` and `IVKUserContext`. | CS.06 |
| **Error Handling** | Map all failures to `PersistenceErrors` and return `VKResult` (CS.01). Raw provider exceptions (e.g., `DbUpdateException`) must not leak. | CS.01 |
| **DI Registration** | `VKPersistenceBlock` must follow the 8-step registration order (BB.03). Use `IVKPersistenceBuilder` as the sole registration point. | BB.03, AP.02 |
| **Configuration** | Validate `VKPersistenceOptions` at startup via `IValidateOptions`. Invalid configs must block application startup. | AP.04, BB.05 |
| **Observability** | Emit metrics/traces under the shared `PersistenceDiagnostics` / `DiagnosticsConstants` namespace. No custom namespaces. | BB.04 |
| **Specifications** | Use composable specification contracts instead of adding custom, single-purpose repository methods. | - |
| **Soft Delete** | Express via marker interface (e.g., `ISoftDeletable`). Filtering logic belongs to the provider, never repositories. | CS.05 |
