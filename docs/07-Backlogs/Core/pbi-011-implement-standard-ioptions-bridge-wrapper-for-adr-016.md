---
id: "#011"
legacy_id: "CORE-013"
category: debt
parent: null
title: Implement standard IOptions Bridge Wrapper for ADR-016
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
  - ADR-016
  docs: []
commits: []
---
# Task: Implement standard IOptions Bridge Wrapper for ADR-016
**ID**: #011 *(Legacy: CORE-013)*
**Status**: 🟡 Medium | #Debt
**Target**: `VK.Blocks.Core -> DependencyInjection/`
**Ref**: ADR-016

## 📝 Description
Create a bridge/wrapper that allows building blocks utilizing the standard 'Action<TOptions>' pattern to integrate with the new immutable 'Func<TOptions, TOptions>' pipeline. This ensures backward compatibility while enforcing the architectural principles of ADR-016.

## ✅ DoD (Definition of Done)
- [x] Implement standard IOptions Bridge Wrapper for ADR-016
- [x] **Assess if an ADR is required (DL.03)**
- [x] Verify changes
- [x] Run tests