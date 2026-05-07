# Task: Mapping SG Integration
**ID**: WEB-001
**Status**: 🔴 High | #Debt
**Target**: `VK.Blocks.Web` -> `WebBlockRegistration.cs`
**Ref**: Rule-10

## 📝 Description
Remove manual registration of mappers in `WebBlockRegistration.cs` once the Source Generator for auto-registration is implemented. This will reduce maintenance overhead.

## ✅ DoD (Definition of Done)
- [ ] Implement or enable Mapping Source Generator
- [ ] Remove manual registration lines in `WebBlockRegistration.cs`
- [ ] Verify build and registration idempotency
