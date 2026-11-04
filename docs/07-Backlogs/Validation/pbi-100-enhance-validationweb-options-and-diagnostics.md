---
id: "#100"
legacy_id: "VALIDATION.WEB-001"
category: debt
parent: null
title: Enhance Validation.Web Options and Diagnostics
module: Validation
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
# Task: Enhance Validation.Web Options and Diagnostics
**ID**: #100 *(Legacy: VALIDATION.WEB-001)*
**Status**: 🟡 Medium | #Debt
**Target**: `src/BuildingBlocks/Validation.Web`
**Ref**: N/A

## 📝 Description
Update VKValidationWebOptions to include 'UseValidationFilter', 'IncludePropertyPath', and 'InvalidStatusCode'. Implement ValidationWebLog in Diagnostics/Internal/ with methods: FilterExecuting, FilterPassed, FilterRejected.

## ✅ DoD (Definition of Done)
- [ ] Enhance Validation.Web Options and Diagnostics
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests