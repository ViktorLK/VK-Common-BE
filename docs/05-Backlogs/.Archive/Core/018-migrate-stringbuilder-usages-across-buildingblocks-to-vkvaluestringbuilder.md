# Task: Migrate StringBuilder usages across BuildingBlocks to VKValueStringBuilder
**ID**: CORE-018
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