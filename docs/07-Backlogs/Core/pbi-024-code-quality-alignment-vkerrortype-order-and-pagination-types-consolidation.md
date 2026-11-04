---
id: '#024'
title: 'Code Quality Alignment: VKErrorType Order and Pagination Types Consolidation'
module: Core
category: debt
parent: 
legacy_id: CORE-024
type: debt
priority: P3
status: icebox
sprint: 
points: 3
rank: 
started: 
carry_over_count: 0
ready: true
estimate: 
created: 2026-05-01
closed: 
refs:
  rules:
  - Core_20260503.md
  - CS.01
  - FS-08
  - BB.01
  docs: []
commits: []
---
# Task: Code Quality Alignment: VKErrorType Order and Pagination Types Consolidation
**ID**: #024 *(Legacy: CORE-024)*
**Status**: 🔵 Low | #Debt
**Target**: `src/BuildingBlocks/Core/Contracts/`, `src/BuildingBlocks/Core/Results/`
**Ref**: Core_20260503.md, CS.01, FS-08, BB.01

## 📝 Description

### 1. Problem / Context (現状の課題と背景)
In the 2026-05-03 architecture audit (Score: 97/100), two minor quality risks were identified:
1. `VKErrorType` enum members are listed in an irregular sequence rather than logical HTTP status code progression (4xx client errors -> 5xx server errors), increasing review friction.
2. Pagination contract types exhibit slight folder segregation: `VKPagedResult` and `VKCursorPagedResult` reside in `Contracts/`, whereas `IVKPagedResult` and `IVKResult` reside in `Results/`.

### 2. Proposed Changes (具体的な改修方針)
- **Step 1**: Reorganize `VKErrorType` declarations to logically mirror ascending HTTP status codes (Validation 400 -> NotFound 404 -> ... -> ServiceUnavailable 503) without altering explicit integer enum values.
- **Step 2**: Consolidate pagination interfaces and record implementations under `Results/` (or cleanly standardize `Contracts/` vs `Results/` per BB.01).
- **Step 3**: Re-run audit checklist to recover deducted score points.

## ✅ DoD (Definition of Done)
- [ ] `VKErrorType` reordered in logical HTTP code progression with zero binary value shifts
- [ ] Pagination contract types consolidated under vertical slice standard
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Clean build and test execution
