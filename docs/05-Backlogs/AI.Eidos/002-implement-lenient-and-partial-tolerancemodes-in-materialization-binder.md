# Task: Implement Lenient and Partial ToleranceModes in Materialization Binder
**ID**: AI.EIDOS-002
**Status**: 🟡 Medium | #Debt
**Target**: `src/BuildingBlocks/AI.Eidos/Materialization/Internal/DefaultMaterializationBinder.cs`
**Ref**: CS.01, AP.01

## 📝 Description
Implement full lenient and partial tolerance modes for DefaultMaterializationBinder. Lenient mode should fallback missing optional properties to defaults; Partial mode should filter out invalid items from collections and harvest partial properties instead of failing the entire contract binding.

## ✅ DoD (Definition of Done)
- [ ] Implement Lenient and Partial ToleranceModes in Materialization Binder
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests