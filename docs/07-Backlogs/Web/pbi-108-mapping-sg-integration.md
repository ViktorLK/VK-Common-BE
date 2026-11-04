---
id: "#108"
legacy_id: "WEB-001"
category: debt
parent: null
title: Mapping SG Integration
module: Web
type: debt
priority: P1
status: todo
sprint: 
estimate: 
created: 2026-05-01
closed: 
refs:
  rules:
  - AP.05
  docs: []
commits: []
---
# Task: Mapping SG Integration
**ID**: #108 *(Legacy: WEB-001)*
**Status**: 🔴 High | #Debt
**Target**: `VK.Blocks.Web` -> `WebBlockRegistration.cs`
**Ref**: AP.05

## 📝 Description
Remove manual registration of mappers in `WebBlockRegistration.cs` once the Source Generator for auto-registration is implemented. This will reduce maintenance overhead.

## ✅ DoD (Definition of Done)
- [ ] Implement or enable Mapping Source Generator
- [ ] Remove manual registration lines in `WebBlockRegistration.cs`
- [ ] Verify build and registration idempotency
