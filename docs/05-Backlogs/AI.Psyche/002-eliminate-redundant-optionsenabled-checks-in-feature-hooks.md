# Task: Eliminate redundant options.Enabled checks in feature hooks
**ID**: AI.PSYCHE-002
**Status**: 🟡 Medium | #Debt
**Target**: `src/BuildingBlocks/AI.Psyche/*/Feature.cs`
**Ref**: BB.06, AP.02

## 📝 Description
The source generator for [VKFeature] already emits 'if (!options.Enabled) return builder;' before calling RegisterFeatureCustom. Hand-written Feature classes across AI.Psyche currently have redundant 'if (!options.Enabled) return;' checks that can be safely unified or cleaned up globally.

## ✅ DoD (Definition of Done)
- [ ] Eliminate redundant options.Enabled checks in feature hooks
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests