---
layer: 3
id: persistence-manifest
scope: building-blocks/persistence
requires: CS.01, CS.02, CS.04, CS.05, CS.06, AP.02, AP.04, BB.03, BB.04, BB.05
---

# Persistence Manifest (L3 — ORM-Agnostic Contracts)

Zero references to any concrete ORM, driver, or SDK. Purely BCL + Core.

## Boundaries

| Boundary | Rule |
|:--|:--|
| **Read / Write** | Separate surfaces. Read = no-tracking, no mutation path. Write = sole mutation path. |
| **Commit** | Exactly one abstraction owns commit. Individual contracts MUST NOT expose independent save/flush. |
| **Transaction** | Cross-boundary / non-default isolation = distinct opt-in concern, never implicit in UoW. |
| **Materialization** | Provider-only hydration contracts MUST NOT be reachable from Domain / App layers. [CS.02] |
| **Audit** | Time + identity sourced exclusively from Core abstractions (`TimeProvider`, `IVKUserContext`). [CS.06] |
| **Error** | Layer speaks its own result/error vocabulary only. Provider exception translation = provider's job. [CS.01] |
| **Soft-Delete** | Declared at entity level. Filtering mechanism = provider concern. |
| **Config** | Validated at startup. Misconfiguration = startup failure, never first-request failure. [AP.04] |
| **Observability** | All diagnostics under a single, shared diagnostics identity. [BB.04] |
| **Registration** | One builder entry point. Providers extend — never introduce parallel entry points. Fixed, deterministic step order. [BB.03] |
| **Extensibility** | New query needs = compose existing contracts. Never grow single-purpose provider-shaped methods. |
| **Provider Neutrality** | Nothing here may assume shape, capability, or limitation of any single provider. |
