---
id: "#016"
legacy_id: "CORE-010"
category: debt
parent: null
title: Implement VKGuard Source Generator for CallerInfo
module: Core
type: debt
priority: P2
status: todo
sprint: 
estimate: 
created: 2026-05-01
closed: 
refs:
  rules:
  - Core README (Future Outlook)
  - ADR-010
  docs: []
commits: []
---
# Task: Implement VKGuard Source Generator for CallerInfo
**ID**: #016 *(Legacy: CORE-010)*
**Status**: 🟡 Medium | #Debt
**Target**: `VK.Blocks.Core -> Guards/`
**Ref**: Core README (Future Outlook), ADR-010

## 📝 Description
Implement a Source Generator to resolve 'CallerArgumentExpression' and parameter metadata at compile-time for 'VKGuard' methods. This aims to eliminate runtime reflection/metadata retrieval and further reduce the overhead of high-frequency guard clauses.

## ✅ DoD (Definition of Done)
- [ ] Implement VKGuard Source Generator for CallerInfo
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests