# Task: Decouple Hardcoded AI Dependencies from DI Source Generator
**ID**: AI-017
**Status**: 🟡 Medium | #Debt
**Target**: `N/A`
**Ref**: DL.04

## 📝 Description
Refactor FeatureArgsEmitter in the DI Source Generator to remove namespace string matching (.AI) and IVKAIArgs/Timeout hardcoding. Instead, detect IVKAIProviderOptions interface implementation via Roslyn Semantic Model.

## ✅ DoD (Definition of Done)
- [ ] Decouple Hardcoded AI Dependencies from DI Source Generator
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests