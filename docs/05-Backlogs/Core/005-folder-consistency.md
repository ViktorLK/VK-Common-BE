# Task: Architecture: Folder Consistency
**ID**: CORE-005
**Status**: 🟡 Medium | #Architecture
**Target**: All Building Blocks -> Folder Structure
**Ref**: Rule-16

## 📝 Description
Standardize the split between `Contracts/` and `Results/`. Decide if `IVKResult` and related types belong in `Contracts` or `Results` and apply consistently across all blocks. Currently mixed.

## ✅ DoD (Definition of Done)
- [ ] Establish a clear rule in ADR or Rule-16
- [ ] Move files to correct folders in all modules
- [ ] Update namespaces and references
