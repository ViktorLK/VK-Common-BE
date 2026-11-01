---
id: "#034"
legacy_id: "AI-005"
category: debt
parent: null
title: Implement Integrated Moderation Decorator
module: AI
type: debt
priority: P1
status: done
sprint: 
estimate: 
created: 2026-05-01
closed: 2026-05-05
refs:
  rules:
  - ADR-008
  docs: []
commits: []
---
# Task: Implement Integrated Moderation Decorator
**ID**: #034 *(Legacy: AI-005)*
**Status**: 🔴 High | #Debt
**Target**: `N/A`
**Ref**: ADR-008

## 📝 Description
Develop the VKGovernanceDecorator to automatically inject IVKModerationEngine checks into Chat and Embedding engines. Support Blocking and Audit-only modes via configuration as defined in ADR-008.

## ✅ DoD (Definition of Done)
- [ ] Implement Integrated Moderation Decorator
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests