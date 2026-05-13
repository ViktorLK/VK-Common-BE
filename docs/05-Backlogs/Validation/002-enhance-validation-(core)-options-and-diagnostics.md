# Task: Enhance Validation (Core) Options and Diagnostics
**ID**: VALIDATION-002
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