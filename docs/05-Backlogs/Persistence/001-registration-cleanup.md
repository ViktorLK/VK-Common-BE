# Task: Persistence Registration Cleanup
**ID**: PERSISTENCE-001
**Status**: 🔴 High | #Debt
**Target**: `VK.Blocks.Persistence` -> Dependency Injection
**Ref**: Rule-18

## 📝 Description
Finalize the removal of any legacy registration methods. Ensure `AddPersistenceBlock` is the sole entry point and correctly handles marker registration.

## ✅ DoD (Definition of Done)
- [ ] Remove legacy registration methods
- [ ] Ensure `AddPersistenceBlock` is used everywhere
- [ ] Verify marker registration
