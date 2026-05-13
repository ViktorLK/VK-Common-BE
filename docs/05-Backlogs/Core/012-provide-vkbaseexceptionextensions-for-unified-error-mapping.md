# Task: Provide VKBaseExceptionExtensions for Unified Error Mapping
**ID**: CORE-012
**Status**: 🟡 Medium | #Debt
**Target**: `VK.Blocks.Core -> Exceptions/`
**Ref**: ADR-011

## 📝 Description
Implement a set of extension methods to facilitate the conversion between standard .NET exceptions, 'VKResult' objects, and the new 'VKBaseException' hierarchy. This helper is essential for migrating legacy modules to the standardized fault modeling defined in ADR-011.

## ✅ DoD (Definition of Done)
- [ ] Provide VKBaseExceptionExtensions for Unified Error Mapping
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests