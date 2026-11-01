---
id: "#013"
legacy_id: "CORE-015"
category: debt
parent: null
title: Refactor Configuration Section Naming to Namespace-Underscore Pattern
module: Core
type: debt
priority: P2
status: dropped
sprint: 
estimate: 
created: 2026-05-01
closed: 2026-05-05
refs:
  rules:
  - AP.04
  - BB.05
  docs: []
commits: []
---
# Task: Refactor Configuration Section Naming to Namespace-Underscore Pattern
**ID**: #013 *(Legacy: CORE-015)*
**Status**: ⚪ Closed | #Obsolete
**Target**: `VK.Blocks.Core.Constants.VKBlocksConstants, IVKBlockMarker`
**Ref**: AP.04, BB.05

## 📝 Description
> [!NOTE]
> **Status: Closed / Obsolete (Not Needed)**
> Current configuration architecture in VK.Blocks has fully adopted standard hierarchical colon-separated sections (e.g., `VKBlocks:AI:Chat`, `VKBlocks:Web:Cors`) and PascalCase block names (e.g., `AIVectorStoreSqlite`), which natively translate to `VKBlocks__AI__Chat` in environment variables across Windows, Linux, and Docker without dot-related conflicts. No ad-hoc underscore transformation utility is required.

## ✅ DoD (Definition of Done)
- [x] Assess requirement and design necessity per ADR/Linux env specs
- [x] Verify that current hierarchical colon pattern satisfies cross-platform compatibility