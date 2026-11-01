---
id: "#020"
legacy_id: "CORE-020"
category: debt
parent: null
title: Roll out Standardized Entry Filtering and Pipeline Framework to Downstream AI Blocks
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
  - ADR-018
  - ADR-019
  - BB.01
  - CS.01
  docs: []
commits: []
---
# Task: Roll out Standardized Entry Filtering and Pipeline Framework to Downstream AI Blocks
**ID**: #020 *(Legacy: CORE-020)*
**Status**: 🟡 Medium | #Debt
**Target**: `src/BuildingBlocks/Core/Filtering/, src/BuildingBlocks/Core/Pipeline/`
**Ref**: ADR-018, ADR-019, BB.01, CS.01

## 📝 Description
### 1. Problem / Context (現状の課題と背景)
ADR-018 introduced `IVKEntryFilter<TItem, TContext>` and `VKFilterVerdict` in `Filtering/`, and ADR-019 introduced `IVKPipeline<TContext>` and `VKPipelineExecutorBase<TContext>` in `Pipeline/`. However, downstream AI modules (e.g., `AI.Corpus` lifecycle filtering, `AI.Psyche` prompt staging) still retain legacy slice-local filter definitions and duplicated pipeline execution algorithms.

### 2. Proposed Changes (具体的な改修方針)
- **Step 1**: Refactor `AI.Corpus` filters (e.g. `IVKKnowledgeLifecycleFilter`) to derive from `IVKEntryFilter<TEntry, TContext>`.
- **Step 2**: Migrate `AI.Psyche` execution pipeline to inherit from `VKPipelineExecutorBase<TContext>`.
- **Step 3**: Standardize cross-cutting middleware (OTel Activity tags, Metrics recording, and Polly resilience) across all pipeline executions via `IVKMiddleware<TContext>`.

## ✅ DoD (Definition of Done)
- [ ] Downstream AI modules successfully migrate to Core `IVKEntryFilter` and `IVKPipeline` abstractions
- [ ] Unified OTel metrics and trace tags propagate across all pipeline stages
- [ ] **Assess if an ADR is required (DL.03)** for downstream module architecture updates
- [ ] Compile-time safety and zero warnings (`BASE-03`, `DL.02`)

## ✅ DoD (Definition of Done)
- [ ] Roll out Standardized Entry Filtering and Pipeline Framework to Downstream AI Blocks
- [ ] **Assess if an ADR is required (`DL.03`)** — 公開契約またはアーキテクチャパターン変更時は `docs/04-ArchitectureDecisionRecords/` に記録
- [ ] Enforce compile-time safety and zero warnings (`BASE-03`, `DL.02`)
- [ ] Verify all 4 execution paths (Happy / NotFound / Permission / Infra) (`DL.01`)