---
id: "#002"
legacy_id: "CORE-002"
category: debt
parent: null
title: VKErrorType Alignment
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
  - CS.01
  docs: []
commits: []
---
# Task: VKErrorType Alignment
**ID**: #002 *(Legacy: CORE-002)*
**Status**: ✅ Completed | #Debt
**Target**: `VK.Blocks.Core` -> `VKErrorType.cs`
**Ref**: CS.01

## 📝 Description
Reorder the `VKErrorType` enum members to match HTTP status code logic (4xx -> 5xx) to improve developer cognitive load.

## ✅ DoD (Definition of Done)
- [x] Reorder enum members
- [x] Verify existing usages
- [x] Run tests
