# Task: Refactor Configuration Section Naming to Namespace-Underscore Pattern
**ID**: CORE-015
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