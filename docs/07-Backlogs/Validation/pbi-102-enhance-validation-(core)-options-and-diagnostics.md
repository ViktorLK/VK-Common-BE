---
id: "#102"
legacy_id: "VALIDATION-002"
category: debt
parent: null
title: Enhance Validation (Core) Options and Diagnostics
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
# Task: Enhance Validation (Core) Options and Diagnostics
**ID**: #102 *(Legacy: VALIDATION-002)*
**Status**: 🟡 Medium | #Debt
**Target**: `src/BuildingBlocks/Validation`
**Ref**: N/A

## 📝 Description
Update VKValidationOptions to include 'ThrowOnValidationFailure' and default 'Enabled' to false. Implement ValidationLog in Diagnostics/Internal/ with methods: ValidationStarted, ValidationSucceeded, ValidationFailed, ValidatorNotFound.

## ✅ DoD (Definition of Done)
- [ ] Enhance Validation (Core) Options and Diagnostics
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests