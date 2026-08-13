# Task: Seal VKBlockBuilder
**ID**: CORE-001
**Status**: ❌ Rejected | #Invalid
**Target**: `VK.Blocks.Core` -> `VKBlockBuilder.cs`
**Ref**: Rule-12

## 📝 Description
`VKBlockBuilder<TMarker>` is currently not marked as `sealed`.
As it is a core framework registration class, it should not allow third-party inheritance.

> ❌ **Rejected Reason**: `VKBlockBuilder` serves as a base class for specific block builders across modules and must support inheritance.

## ✅ DoD (Definition of Done)
- [x] Rejected: `VKBlockBuilder` must remain inheritable.

