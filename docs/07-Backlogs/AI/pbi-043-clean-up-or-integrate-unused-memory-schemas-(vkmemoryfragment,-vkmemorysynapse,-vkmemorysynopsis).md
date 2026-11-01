---
id: "#043"
legacy_id: "AI-014"
category: debt
parent: null
title: Clean up or integrate unused memory schemas (VKMemoryFragment, VKMemorySynapse, VKMemorySynopsis)
module: AI
type: debt
priority: P2
status: done
sprint: 
estimate: 
created: 2026-05-01
closed: 2026-05-05
refs:
  rules:
  - Rule DL.04 / AP.03
  docs: []
commits: []
---
# Task: Clean up or integrate unused memory schemas (VKMemoryFragment, VKMemorySynapse, VKMemorySynopsis)
**ID**: #043 *(Legacy: AI-014)*
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