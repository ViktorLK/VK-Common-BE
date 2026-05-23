# Task: Clean up or integrate unused memory schemas (VKMemoryFragment, VKMemorySynapse, VKMemorySynopsis)
**ID**: AI-014
**Status**: 🟡 Medium | #Debt
**Target**: `src/BuildingBlocks/AI.Cognitive/Memory/`
**Ref**: Rule DL.04 / AP.03

## 📝 Description
The models VKMemoryFragment, VKMemorySynapse, and VKMemorySynopsis are currently defined in AI.Cognitive\Memory but have zero active usages across the codebase. We need to decide whether to integrate them into their planned subsystems (e.g. Prompt Tapestry/RAG prompt injection, Memory Graph, and sleep memory crystallization) or safely prune them to keep the cognitive codebase clean and minimal.

## ✅ DoD (Definition of Done)
- [ ] Clean up or integrate unused memory schemas (VKMemoryFragment, VKMemorySynapse, VKMemorySynopsis)
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests