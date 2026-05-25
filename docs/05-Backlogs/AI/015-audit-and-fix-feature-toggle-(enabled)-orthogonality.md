# Task: Audit and Fix Feature Toggle (Enabled) Orthogonality
**ID**: BUILDINGBLOCKS.AI-001
**Status**: 🔴 High | #Debt
**Target**: `All *Feature.cs and their Options`
**Ref**: N/A

## 📝 Description
Perform a systematic audit across all VK.Blocks features (especially AI sub-features) to ensure complete orthogonality of the `Enabled` toggles.
- Verify that features can be turned on or off independently without causing DI resolution failures (e.g., missing Singletons/Scoped services).
- Replace hard constructor dependencies with optional dependencies (`IServiceProvider`, nullable `?`, or `IEnumerable<T>`) where cross-feature dependencies exist.
- Write unit tests or a startup validation mechanism that iterates through various permutations of boolean toggles to guarantee successful application startup (`BuildServiceProvider(validateScopes: true)`).

## ✅ DoD (Definition of Done)
- [ ] Audit and Fix Feature Toggle (Enabled) Orthogonality
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests