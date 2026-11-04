---
id: "#019"
legacy_id: "CORE-019"
category: debt
parent: null
title: DI Options Registration Concurrency and Thread-Safety Hardening
module: Core
type: debt
priority: P2
status: todo
sprint: 
estimate: 
created: 2026-05-01
closed: 
refs:
  rules:
  - ADR-017
  - AP.02
  - Core_20260503.md
  docs: []
commits: []
---
# Task: DI Options Registration Concurrency and Thread-Safety Hardening
**ID**: #019 *(Legacy: CORE-019)*
**Status**: 🟡 Medium | #Debt
**Target**: `src/BuildingBlocks/Core/DependencyInjection/VKBlockOptionsRegistrationExtensions.cs`
**Ref**: ADR-017, AP.02, Core_20260503.md

## 📝 Description
### 1. Problem / Context (現状の課題と背景)
In `AddVKBlockOptions`, `services.Replace` is used to override `IOptionsFactory<T>` with `VKBlockOptionsFactory<T>` to support immutable options with `Func<T, T>` and `with` expressions (ADR-016, ADR-017). However, `services.Replace` is not thread-safe. If building block registration is invoked across concurrent threads during host bootstrap, race conditions can occur. Currently, this relies on a documentation disclaimer requiring synchronous single-threaded startup execution.

### 2. Proposed Changes (具体的な改修方針)
- **Step 1**: Analyze registration concurrency risks in host bootstrapping pipelines.
- **Step 2**: Introduce a lock-protected registration guard or deterministic synchronization wrapper in `VKBlockOptionsRegistrationExtensions.cs` to prevent concurrent corruption of `IServiceCollection`.
- **Step 3**: Add runtime diagnostics or debug assertion warnings when concurrent registrations are detected during container building.

## ✅ DoD (Definition of Done)
- [ ] Provide concurrency analysis and implement synchronization guards in `VKBlockOptionsRegistrationExtensions`
- [ ] **Assess if an ADR is required (DL.03)** — assess impact on DI bootstrap performance
- [ ] Ensure compile-time safety and zero warnings (`BASE-03`, `DL.02`)
- [ ] Verify thread-safety under multi-threaded configuration scenarios

## ✅ DoD (Definition of Done)
- [ ] DI Options Registration Concurrency and Thread-Safety Hardening
- [ ] **Assess if an ADR is required (`DL.03`)** — 公開契約またはアーキテクチャパターン変更時は `docs/04-ArchitectureDecisionRecords/` に記録
- [ ] Enforce compile-time safety and zero warnings (`BASE-03`, `DL.02`)
- [ ] Verify all 4 execution paths (Happy / NotFound / Permission / Infra) (`DL.01`)