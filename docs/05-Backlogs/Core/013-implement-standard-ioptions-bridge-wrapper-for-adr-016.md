# Task: Implement standard IOptions Bridge Wrapper for ADR-016
**ID**: CORE-013
**Status**: 🟡 Medium | #Debt
**Target**: `VK.Blocks.Core -> DependencyInjection/`
**Ref**: ADR-016

## 📝 Description
Create a bridge/wrapper that allows building blocks utilizing the standard 'Action<TOptions>' pattern to integrate with the new immutable 'Func<TOptions, TOptions>' pipeline. This ensures backward compatibility while enforcing the architectural principles of ADR-016.

## ✅ DoD (Definition of Done)
- [ ] Implement standard IOptions Bridge Wrapper for ADR-016
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests