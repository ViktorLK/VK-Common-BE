# Task: Optimize configuration path resolution in VKFeatureGenerator
**ID**: CORE-016
**Status**: 🟡 Medium | #Debt
**Target**: `VKFeatureGenerator.cs`
**Ref**: DL.04, BB.01

## 📝 Description
Optimize configuration path resolution in VKFeatureGenerator. Currently, it uses a compile-time namespace and assembly overlap algorithm to trace the parent/child lineage (e.g., TokenicsFeature -> VKAIBlock) because features do not declare their parent block in handwritten source files. Refactor the generator to leverage explicit block markers, metadata attributes, or specialized feature relationships on Options classes to simplify and streamline parent tracing without relying on namespace segments.

## ✅ DoD (Definition of Done)
- [ ] Optimize configuration path resolution in VKFeatureGenerator
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests