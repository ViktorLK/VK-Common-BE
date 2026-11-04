---
id: "#021"
legacy_id: "CORE-021"
category: debt
parent: null
title: Expand FrozenCollections Across Static Metadata Caches
module: Core
type: debt
priority: P3
status: icebox
sprint: 
estimate: 
created: 2026-05-01
closed: 
refs:
  rules:
  - ADR-010
  - Core_20260503.md
  docs: []
commits: []
---
# Task: Expand FrozenCollections Across Static Metadata Caches
**ID**: #021 *(Legacy: CORE-021)*
**Status**: 🔵 Low | #Debt
**Target**: `src/BuildingBlocks/Core/Domain/, src/BuildingBlocks/Core/Common/Shared/`
**Ref**: ADR-010, Core_20260503.md

## 📝 Description
### 1. Problem / Context (現状の課題と背景)
In ADR-010 and the Core 2026-05-03 audit, `PropertySecurityCache` successfully adopted .NET 8+ `FrozenDictionary` to provide zero-allocation, O(1) metadata lookup. However, other static read-heavy caches in Core, such as entity capability caches (`VKEntityMetadata._capabilityCache`), still rely on `ConcurrentDictionary`, which incurs higher lookup overhead and synchronization memory footprints after startup.

### 2. Proposed Changes (具体的な改修方針)
- **Step 1**: Audit all static dictionary caches across `VK.Blocks.Core` (`Domain/`, `Security/`, `Context/`).
- **Step 2**: Implement post-startup freezing mechanism converting static caches to `FrozenDictionary<TKey, TValue>` and `FrozenSet<T>`.
- **Step 3**: Benchmark lookup latency and memory allocations in hot paths.

## ✅ DoD (Definition of Done)
- [ ] Static lookup caches in Core utilize `FrozenDictionary` / `FrozenSet`
- [ ] Hot path benchmarks confirm zero allocation and reduced lookup latency
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Clean build and test execution

## ✅ DoD (Definition of Done)
- [ ] Expand FrozenCollections Across Static Metadata Caches
- [ ] **Assess if an ADR is required (`DL.03`)** — 公開契約またはアーキテクチャパターン変更時は `docs/04-ArchitectureDecisionRecords/` に記録
- [ ] Enforce compile-time safety and zero warnings (`BASE-03`, `DL.02`)
- [ ] Verify all 4 execution paths (Happy / NotFound / Permission / Infra) (`DL.01`)