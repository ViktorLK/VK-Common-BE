---
id: '#093'
title: Persistence Registration Cleanup
module: Persistence
category: debt
parent: 
legacy_id: PERSISTENCE-001
type: debt
priority: P1
status: todo
sprint: S-2026-44
points: 3
rank: 
started: 
carry_over_count: 0
ready: true
estimate: 
created: 2026-05-01
closed: 
refs:
  rules:
  - BB.03, AP.02
  docs: []
commits: []
---
# Task: Persistence Registration Cleanup
**ID**: #093 *(Legacy: PERSISTENCE-001)*
**Status**: 🔴 High | #Debt
**Target**: `VK.Blocks.Persistence` -> Dependency Injection
**Ref**: BB.03, AP.02

## 📝 Description
Finalize the removal of any legacy registration methods. Ensure `AddPersistenceBlock` is the sole entry point and correctly handles marker registration.

### 1. Problem / Context (現状の課題と背景)
Persistence module contains deprecated registration methods and non-standard DI hooks that need cleanup to avoid confusing consumers and prevent dual-registration issues.

### 2. Proposed Changes (具体的な改修方針)
- Remove legacy registration extension methods across Persistence.
- Enforce `AddPersistenceBlock` as the sole entry point with strict marker registration per BB.02 / BB.03.
- Verify DI idempotency with unit tests.

## ✅ DoD (Definition of Done)
- [ ] Remove legacy registration methods
- [ ] Ensure `AddPersistenceBlock` is used everywhere
- [ ] Verify marker registration
