---
id: "#052"
legacy_id: "AI.PSYCHE-002"
category: debt
parent: null
title: Eliminate redundant options.Enabled checks in feature hooks
module: AI
type: debt
priority: P2
status: icebox
sprint: 
estimate: 
created: 2026-05-01
closed: 
refs:
  rules:
  - BB.06
  - AP.02
  docs: []
commits: []
---
# Task: Eliminate redundant options.Enabled checks in feature hooks
**ID**: #052 *(Legacy: AI.PSYCHE-002)*
**Status**: 🟡 Medium | #Debt
**Target**: `src/BuildingBlocks/AI.Psyche/*/Feature.cs`
**Ref**: BB.06, AP.02

## 📝 Description
The source generator for [VKFeature] already emits 'if (!options.Enabled) return builder;' before calling RegisterFeatureCustom. Hand-written Feature classes across AI.Psyche currently have redundant 'if (!options.Enabled) return;' checks that can be safely unified or cleaned up globally.

## ✅ DoD (Definition of Done)
- [ ] Eliminate redundant options.Enabled checks in feature hooks
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests