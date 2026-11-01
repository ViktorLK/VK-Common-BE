---
id: "#023"
legacy_id: "CORE-023"
category: debt
parent: null
title: Full Native AOT Compilation Readiness and Trimming Audit
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
  - Core README
  - ARCH-03
  - AP.01
  docs: []
commits: []
---
# Task: Full Native AOT Compilation Readiness and Trimming Audit
**ID**: #023 *(Legacy: CORE-023)*
**Status**: 🟡 Medium | #Debt
**Target**: `src/BuildingBlocks/Core/VK.Blocks.Core.csproj, src/BuildingBlocks/Core/Serialization/`
**Ref**: Core README, ARCH-03, AP.01

## 📝 Description
### 1. Problem / Context (現状の課題と背景)
Core is the root foundation of the VK.Blocks framework. Cloud-native deployments, container microservices, and serverless runtimes increasingly demand Native AOT compilation. Although Core adheres to zero-dependency and zero-reflection principles, it lacks formal `<IsAotCompatible>true</IsAotCompatible>` enforcement and comprehensive trimming analyzer verification.

### 2. Proposed Changes (具体的な改修方針)
- **Step 1**: Enable `<IsAotCompatible>true</IsAotCompatible>` and `<EnableTrimAnalyzer>true</EnableTrimAnalyzer>` in `VK.Blocks.Core.csproj`.
- **Step 2**: Audit all serialization pathways (`IVKJsonSerializer`, `SystemTextJsonSerializer`) to enforce `JsonSerializerContext` Source Generators instead of runtime reflection.
- **Step 3**: Verify generic type caches, static abstract interfaces, and throw helpers against ILLink trimming warnings (IL2026, IL2091).

## ✅ DoD (Definition of Done)
- [ ] `VK.Blocks.Core.csproj` builds cleanly with `<IsAotCompatible>true</IsAotCompatible>` and zero trimming warnings
- [ ] All JSON serialization paths are trim-safe and use source-generated contexts
- [ ] **Assess if an ADR is required (DL.03)** — record AOT compatibility baseline
- [ ] End-to-end AOT publish verification succeeded

## ✅ DoD (Definition of Done)
- [ ] Full Native AOT Compilation Readiness and Trimming Audit
- [ ] **Assess if an ADR is required (`DL.03`)** — 公開契約またはアーキテクチャパターン変更時は `docs/04-ArchitectureDecisionRecords/` に記録
- [ ] Enforce compile-time safety and zero warnings (`BASE-03`, `DL.02`)
- [ ] Verify all 4 execution paths (Happy / NotFound / Permission / Infra) (`DL.01`)