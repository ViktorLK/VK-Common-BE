---
id: "#005"
legacy_id: "CORE-005"
category: debt
parent: null
title: 'Architecture: Folder Consistency'
module: Core
type: debt
priority: P2
status: done
sprint: 
estimate: 
created: 2026-05-01
closed: 2026-05-05
refs:
  rules:
  - BB.01
  docs: []
commits: []
---
# Task: Architecture: Folder Consistency
**ID**: #005 *(Legacy: CORE-005)*
**Status**: ✅ Completed | #Architecture
**Target**: All Building Blocks -> Folder Structure
**Ref**: BB.01

## 📝 Description
Standardize the split between `Contracts/` and `Results/`. Decide if `IVKResult` and related types belong in `Contracts` or `Results` and apply consistently across all blocks. Currently mixed.

## ✅ DoD (Definition of Done)
- [x] Establish a clear rule in ADR or BB.01
- [x] Move files to correct folders in all modules
- [x] Update namespaces and references

