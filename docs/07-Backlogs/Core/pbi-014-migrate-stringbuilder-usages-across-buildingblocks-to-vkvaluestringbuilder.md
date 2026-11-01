---
id: "#014"
legacy_id: "CORE-018"
category: debt
parent: null
title: Migrate StringBuilder usages across BuildingBlocks to VKValueStringBuilder
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
  - CS.04
  docs: []
commits: []
---
# Task: Migrate StringBuilder usages across BuildingBlocks to VKValueStringBuilder
**ID**: #014 *(Legacy: CORE-018)*
**Status**: ✅ Completed | #Debt
**Target**: `VKValueStringBuilder`
**Ref**: CS.04

## 📝 Description
Refactor all eligible local StringBuilder instantiations across BuildingBlocks to use VKValueStringBuilder to ensure zero/low heap allocations per CS.04 rules.

## ✅ DoD (Definition of Done)
- [x] Migrate StringBuilder usages across BuildingBlocks to VKValueStringBuilder
- [x] **Assess if an ADR is required (DL.03)**
- [x] Verify changes
- [x] Run tests