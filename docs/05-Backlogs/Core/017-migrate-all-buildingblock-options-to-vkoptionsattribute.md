# Task: Migrate all BuildingBlock Options to VKOptionsAttribute
**ID**: CORE-017
**Status**: 🟡 Medium | #Debt
**Target**: `VKOptionsAttribute`
**Ref**: DL.04

## 📝 Description
Refactor all BuildingBlock Options classes to decorate [VKOptions] directly on Options records and clean up ArgsGenerationMode from [VKFeature] attributes across all modules.

## ✅ DoD (Definition of Done)
- [ ] Migrate all BuildingBlock Options to VKOptionsAttribute
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests