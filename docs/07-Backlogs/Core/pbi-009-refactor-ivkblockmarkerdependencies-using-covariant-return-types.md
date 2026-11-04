---
id: "#009"
legacy_id: "CORE-011"
category: debt
parent: null
title: Refactor IVKBlockMarker.Dependencies using Covariant Return Types
module: Core
type: debt
priority: P3
status: done
sprint: 
estimate: 
created: 2026-05-01
closed: 2026-05-05
refs:
  rules:
  - Core README (Future Outlook)
  - ADR-008
  docs: []
commits: []
---
# Task: Refactor IVKBlockMarker.Dependencies using Covariant Return Types
**ID**: #009 *(Legacy: CORE-011)*
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