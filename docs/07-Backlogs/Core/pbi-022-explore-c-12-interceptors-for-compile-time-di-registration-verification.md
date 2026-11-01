---
id: "#022"
legacy_id: "CORE-022"
category: debt
parent: null
title: Explore C# 12 Interceptors for Compile-Time DI Registration Verification
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
  - Core_20260503.md
  - BB.03
  docs: []
commits: []
---
# Task: Explore C# 12 Interceptors for Compile-Time DI Registration Verification
**ID**: #022 *(Legacy: CORE-022)*
**Status**: 🔵 Low | #Debt
**Target**: `src/BuildingBlocks/Core/DependencyInjection/VKBlockQueryExtensions.cs`
**Ref**: Core_20260503.md, BB.03

## 📝 Description
### 1. Problem / Context (現状の課題と背景)
Currently, `IsVKBlockRegistered` and `EnsureVKBlockRegistered` perform a linear search over `IServiceCollection` (`services.Any(d => d.ImplementationInstance is VKBlockRuntimeMarker ...)`). In enterprise applications with hundreds of service descriptors, repetitive prerequisite checks incur redundant iteration costs during host startup.

### 2. Proposed Changes (具体的な改修方針)
- **Step 1**: Research C# 12/13 interceptor capabilities and Roslyn Source Generator hooks for DI registrations.
- **Step 2**: Prototype an interceptor or hashed lookup mechanism that replaces linear `services.Any()` with compile-time or O(1) hash lookup.
- **Step 3**: Verify compatibility with Native AOT and trimmed publishing profiles.

## ✅ DoD (Definition of Done)
- [ ] Prototype or implement compile-time / O(1) registration checks replacing linear scans
- [ ] Performance measurement demonstrating startup latency improvement
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Compile-time safety and zero warnings (`BASE-03`, `DL.02`)

## ✅ DoD (Definition of Done)
- [ ] Explore C# 12 Interceptors for Compile-Time DI Registration Verification
- [ ] **Assess if an ADR is required (`DL.03`)** — 公開契約またはアーキテクチャパターン変更時は `docs/04-ArchitectureDecisionRecords/` に記録
- [ ] Enforce compile-time safety and zero warnings (`BASE-03`, `DL.02`)
- [ ] Verify all 4 execution paths (Happy / NotFound / Permission / Infra) (`DL.01`)