---
id: "#012"
legacy_id: "CORE-014"
category: debt
parent: null
title: Refactor Legacy Exception Throw Sites in Core Library
module: Core
type: debt
priority: P1
status: done
sprint: 
estimate: 
created: 2026-05-01
closed: 2026-05-05
refs:
  rules:
  - ADR-011
  docs: []
commits: []
---
# Task: Refactor Legacy Exception Throw Sites in Core Library
**ID**: #012 *(Legacy: CORE-014)*
**Status**: 🔴 High | #Debt
**Target**: `VK.Blocks.Core (All sub-modules)`
**Ref**: ADR-011

## 📝 Description
Perform a comprehensive audit and refactoring of all internal 'throw' statements within the 'VK.Blocks.Core' library. Replace generic 'Exception' or 'InvalidOperationException' calls with appropriate 'VKBaseException' derivatives (e.g., 'VKDependencyException', 'VKValidationException') to ensure consistency with ADR-011.

## ✅ DoD (Definition of Done)
- [x] Refactor Legacy Exception Throw Sites in Core Library
- [x] **Assess if an ADR is required (DL.03)**
- [x] Verify changes
- [x] Run tests