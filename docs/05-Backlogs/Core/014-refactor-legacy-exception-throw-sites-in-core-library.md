# Task: Refactor Legacy Exception Throw Sites in Core Library
**ID**: CORE-014
**Status**: 🔴 High | #Debt
**Target**: `VK.Blocks.Core (All sub-modules)`
**Ref**: ADR-011

## 📝 Description
Perform a comprehensive audit and refactoring of all internal 'throw' statements within the 'VK.Blocks.Core' library. Replace generic 'Exception' or 'InvalidOperationException' calls with appropriate 'VKBaseException' derivatives (e.g., 'VKDependencyException', 'VKValidationException') to ensure consistency with ADR-011.

## ✅ DoD (Definition of Done)
- [ ] Refactor Legacy Exception Throw Sites in Core Library
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests