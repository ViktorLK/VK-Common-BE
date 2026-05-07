# Task: Unify Options Parameter to `configure`
**ID**: CORE-003
**Status**: 🔴 High | #Debt
**Target**: All Building Blocks -> Options Extensions
**Ref**: Rule-20, ADR-016

## 📝 Description
Align all `Func<TOptions, TOptions>` parameters to `configure` (currently mixed with `transform`). Requires updating BB.05 and refactoring affected blocks.

## ✅ DoD (Definition of Done)
- [ ] Update BB.05 in documentation
- [ ] Refactor all `AddVK...Block` methods to use `configure`
- [ ] Verify build and tests

