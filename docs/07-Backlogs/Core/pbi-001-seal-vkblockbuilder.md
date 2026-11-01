---
id: "#001"
legacy_id: "CORE-001"
category: debt
parent: null
title: Seal VKBlockBuilder
module: Core
type: debt
priority: P2
status: dropped
sprint: 
estimate: 
created: 2026-05-01
closed: 2026-05-05
refs:
  rules:
  - AP.01
  docs: []
commits: []
---
# Task: Seal VKBlockBuilder
**ID**: #001 *(Legacy: CORE-001)*
**Status**: ❌ Rejected | #Invalid
**Target**: `VK.Blocks.Core` -> `VKBlockBuilder.cs`
**Ref**: AP.01

## 📝 Description
`VKBlockBuilder<TMarker>` is currently not marked as `sealed`.
As it is a core framework registration class, it should not allow third-party inheritance.

> ❌ **Rejected Reason**: `VKBlockBuilder` serves as a base class for specific block builders across modules and must support inheritance.

## ✅ DoD (Definition of Done)
- [x] Rejected: `VKBlockBuilder` must remain inheritable.

