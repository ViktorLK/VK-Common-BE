# Task: Authorization DI Normalization
**ID**: AUTHORIZATION-001
**Status**: 🔴 High | #Normalization
**Target**: `VK.Blocks.Authorization` -> Dependency Injection
**Ref**: Rule-18

## 📝 Description
Ensure all Authorization sub-features (Roles, Permissions, etc.) strictly follow the Wrapper -> Core registration sequence without any async-over-sync calls.

## ✅ DoD (Definition of Done)
- [ ] Refactor Roles registration
- [ ] Refactor Permissions registration
- [ ] Verify idempotency and synchronous execution
