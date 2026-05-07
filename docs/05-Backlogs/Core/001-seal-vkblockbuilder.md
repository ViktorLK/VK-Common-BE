# Task: Seal VKBlockBuilder
**ID**: CORE-001
**Status**: 🔴 High | #Debt
**Target**: `VK.Blocks.Core` -> `VKBlockBuilder.cs`
**Ref**: Rule-12

## 📝 Description
`VKBlockBuilder<TMarker>` is currently not marked as `sealed`.
As it is a core framework registration class, it should not allow third-party inheritance.

## ✅ DoD (Definition of Done)
- [ ] Add the `sealed` keyword.
- [ ] Ensure extension methods under `DependencyInjection/` compile correctly.
- [ ] Run tests to confirm no regressions.
