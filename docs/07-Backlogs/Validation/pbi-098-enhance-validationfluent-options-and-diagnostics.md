---
id: "#098"
legacy_id: "VALIDATION.FLUENT-001"
category: debt
parent: null
title: Enhance Validation.Fluent Options and Diagnostics
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
# Task: Enhance Validation.Fluent Options and Diagnostics
**ID**: #098 *(Legacy: VALIDATION.FLUENT-001)*
**Status**: 🟡 Medium | #Debt
**Target**: `src/BuildingBlocks/Validation.Fluent`
**Ref**: N/A

## 📝 Description
Update VKFluentValidationOptions to include 'ValidateOnlyAnnotatedProperties' and 'CascadeMode'. Implement FluentValidationLog in Diagnostics/Internal/ with methods: FluentValidatorResolved, FluentValidatorMissing.

## ✅ DoD (Definition of Done)
- [ ] Enhance Validation.Fluent Options and Diagnostics
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests