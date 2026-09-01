# Task: Refactor IVKBlockMarker.Dependencies using Covariant Return Types
**ID**: CORE-011
**Status**: 🔵 Low | #Debt
**Target**: `VK.Blocks.Core -> Contracts/IVKBlockMarker.cs`
**Ref**: Core README (Future Outlook), ADR-008

## 📝 Description
Enhance the type safety of building block dependency declarations by using C# Covariant Return Types on the 'Dependencies' property of 'IVKBlockMarker'. This ensures that specialized markers can provide more specific type information to the DI traversal logic.

## ✅ DoD (Definition of Done)
- [x] Refactor IVKBlockMarker.Dependencies using Covariant Return Types
- [x] **Assess if an ADR is required (DL.03)**
- [x] Verify changes
- [x] Run tests