---
id: "#003"
legacy_id: "CORE-003"
category: debt
parent: null
title: Unify Options Parameter to `transform` (ADR-016 Functional Alignment)
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
  - BB.05
  - ADR-016
  docs: []
commits: []
---
# Task: Unify Options Parameter to `transform` (ADR-016 Functional Alignment)
**ID**: #003 *(Legacy: CORE-003)*
**Status**: 🟢 Completed | #Debt
**Target**: All Building Blocks -> Options Extensions
**Ref**: BB.05, ADR-016

## 📝 Description
Align all `Func<TOptions, TOptions>` parameters to `transform` across all Building Blocks and Source Generators per ADR-016 immutable functional transformation pattern.

## ✅ DoD (Definition of Done)
- [x] Update BB.05 in documentation to `Func<T, T> transform`
- [x] Standardize all `AddVK...Block` and `Register` methods to use `transform` for immutable functional transformation
- [x] Verify build and tests pass (330/330 passing)

