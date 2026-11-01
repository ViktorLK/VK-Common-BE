---
id: "#054"
legacy_id: "AI.EIDOS-002"
category: debt
parent: null
title: Implement Lenient and Partial ToleranceModes in Materialization Binder
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
  - CS.01
  - AP.01
  docs: []
commits: []
---
# Task: Implement Lenient and Partial ToleranceModes in Materialization Binder
**ID**: #054 *(Legacy: AI.EIDOS-002)*
**Status**: 🟡 Medium | #Debt
**Target**: `src/BuildingBlocks/AI.Eidos/Materialization/Internal/DefaultMaterializationBinder.cs`
**Ref**: CS.01, AP.01

## 📝 Description
Implement full lenient and partial tolerance modes for DefaultMaterializationBinder. Lenient mode should fallback missing optional properties to defaults; Partial mode should filter out invalid items from collections and harvest partial properties instead of failing the entire contract binding.

## ✅ DoD (Definition of Done)
- [ ] Implement Lenient and Partial ToleranceModes in Materialization Binder
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests