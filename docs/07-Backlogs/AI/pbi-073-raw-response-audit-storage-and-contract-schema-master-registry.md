---
id: "#073"
legacy_id: "AI.EIDOS-007"
category: debt
parent: null
title: Raw Response Audit Storage and Contract Schema Master Registry
module: AI
type: debt
priority: P3
status: icebox
sprint: 
estimate: 
created: 2026-05-01
closed: 
refs:
  rules:
  - CS.08
  - AP.07
  docs: []
commits: []
---
# Task: Raw Response Audit Storage and Contract Schema Master Registry
**ID**: #073 *(Legacy: AI.EIDOS-007)*
**Status**: 🔵 Low | #Debt
**Target**: `BuildingBlocks/AI.Eidos.EFCore`
**Ref**: CS.08, AP.07

## 📝 Description
Design and implement an orthogonal 1:1 append-only Raw Response Audit table (VK_AI_Eidos_Echo_Raw / VK_AI_Psyche_Echo_Raw) to store raw LLM JSON outputs and execution metadata for debugging and dataset curation without polluting core conversation memory. Also introduce a Contract Schema Master registry table (VK_AI_Eidos_Contract) with schema fingerprinting and versioning to prevent schema drift and enable time-travel debugging.

## ✅ DoD (Definition of Done)
- [ ] Raw Response Audit Storage and Contract Schema Master Registry
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests