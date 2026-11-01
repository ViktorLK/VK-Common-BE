---
id: '#081'
title: Authorization DI Normalization
module: Authorization
category: debt
parent: 
legacy_id: AUTHORIZATION-001
type: debt
priority: P1
status: done
sprint: S-2026-44
points: 3
rank: 
started: 2026-11-01
carry_over_count: 0
ready: true
estimate: 
created: 2026-05-01
closed: '2026-11-01'
refs:
  rules:
  - BB.03, AP.02
  docs: []
commits: []
---
# Task: Authorization DI Normalization
**ID**: #081 *(Legacy: AUTHORIZATION-001)*
**Status**: 🔴 High | #Normalization
**Target**: `VK.Blocks.Authorization` -> Dependency Injection
**Ref**: BB.03, AP.02

## 📝 Description
Ensure all Authorization sub-features (Roles, Permissions, etc.) strictly follow the Wrapper -> Core registration sequence without any async-over-sync calls.

### 1. Problem / Context (現状の課題と背景)
Currently, Authorization sub-features and extension methods do not strictly follow the standard DI registration order (AP.02, BB.03). Some registrations risk redundant additions or lack marker-based checks.

### 2. Proposed Changes (具体的な改修方針)
- Align all Authorization sub-features (Roles, Permissions, etc.) to strictly follow the Wrapper -> Core registration sequence without async-over-sync calls.
- Standardize idempotency using `TryAdd` semantics and marker verification.

## ✅ DoD (Definition of Done)
- [x] Refactor Roles registration
- [x] Refactor Permissions registration
- [x] Verify idempotency and synchronous execution
