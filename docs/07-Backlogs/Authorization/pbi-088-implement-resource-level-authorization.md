---
id: "#088"
legacy_id: "AUTHORIZATION-008"
category: debt
parent: null
title: Implement Resource-Level Authorization
module: Authorization
type: debt
priority: P2
status: todo
sprint: 
estimate: 
created: 2026-05-01
closed: 
refs:
  rules: []
  docs: []
commits: []
---
# Task: Implement Resource-Level Authorization
**ID**: #088 *(Legacy: AUTHORIZATION-008)*
**Status**: 🟡 Medium | #Debt
**Target**: `N/A`
**Ref**: N/A

## 📝 Description
Provide generic interfaces (e.g., IVKResourceAuthorizationHandler<TResource>) and routing mechanism in Authorization block to support aggregate-level and entity-level permission checks (e.g., check if the user is the owner of the resource) without modifying the core evaluation pipeline.

## ✅ DoD (Definition of Done)
- [ ] Implement Resource-Level Authorization
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests