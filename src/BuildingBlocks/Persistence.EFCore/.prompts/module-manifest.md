---
layer: 3
id: persistence-efcore-manifest
scope: building-blocks/persistence/efcore
requires: CS.01, CS.02, CS.05, AP.02, AP.04, BB.03, BB.04, BB.05, BB.06
depends-on: persistence-manifest
---

# Persistence.EFCore Manifest (L3 — EF Core Provider-Agnostic Implementation)

Concrete EF Core implementation of `persistence-manifest`. References EF Core core package ONLY — zero concrete provider packages (Sqlite/SqlServer/Cosmos).

> All parent boundaries from `persistence-manifest` apply without exception. Rules below are EFCore-specific additions only.

## Dependency Topology

- Each dotted segment = real, independently referenceable assembly. No empty pass-through packages.
- Provider `Use*` calls belong exclusively to provider packages.

## Registration

| Constraint | Rule |
|:--|:--|
| Single DbContext-scoped entry point. Raw + fluent styles composable, not exclusive. | AP.02 |
| Config needing resolved services → deferred to provider-resolution time. Gate config → decided at registration time from materialized Options. | BB.03 |
| Providers extend registered DbContext via supported composition — never re-invoke base entry point. | — |
| Exactly one provider per DbContext. Violation = registration-time failure with clear diagnostic. | — |
| Provider selection scoped per-DbContext, not per-Block or per-application. | — |
| Block + Feature registration = idempotent. Double-registration of same context type = deterministic fail. | BB.03 |
| Interceptor order: deterministic + documented. Framework interceptors before consumer interceptors. Consumers append only. | BB.03 |

## Options

| Constraint | Rule |
|:--|:--|
| Cross-context behavior + Feature switches live here. Provider-specific Options → provider package. | BB.05 |
| All Options via Block's dual-registration mechanism. No ad-hoc bypass. | BB.05, AP.04 |
| Frozen by default. Hot-reload = explicit documented exception. | AP.04 |
| Multi-level switch resolution order (Feature vs Block) = explicit + documented. | AP.04, BB.05 |

## Lifecycle & Cross-Cutting

| Constraint | |
|:--|:--|
| No-op lifecycle processor as safe default. Real processor only when Feature switch effectively enabled. | |
| Bulk ops (`ExecuteUpdate`/`ExecuteDelete`) + raw SQL bypass change-tracking & interceptors. Document bypass risk; provide explicit opt-in escape hatch. | |
| Concurrency: `DbUpdateConcurrencyException` → dedicated `PersistenceErrors` variant (not generic failure). | CS.01 |

## Infrastructure Boundaries

| Constraint | Rule |
|:--|:--|
| ConnectionString via Options infra, never direct `IConfiguration` read. Multi-tenant dynamic switching = Feature extension point. | AP.04 |
| DbContext pooling = opt-in. When enabled, interceptors + lifecycle processors MUST handle state-reset between reuse. | — |
| Migration out of scope. No shipped migrations, no auto-apply, no `EnsureCreated`. | — |
| Slow-query thresholds = Block Options, not hard-coded. EF `DiagnosticSource` events MUST NOT be suppressed. | BB.04 |
| Test-support utilities in dedicated test package only. No test code in production assembly. | — |
