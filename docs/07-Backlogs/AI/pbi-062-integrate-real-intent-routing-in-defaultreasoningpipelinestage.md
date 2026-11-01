---
id: "#062"
legacy_id: "AI.COGNITIVE-003"
category: debt
parent: null
title: Integrate real intent routing in DefaultReasoningPipelineStage
module: AI
type: debt
priority: P2
status: icebox
sprint: 
estimate: 
created: 2026-05-01
closed: 
refs:
  rules:
  - DL.04
  - CS.03
  docs: []
commits: []
---
# Task: Integrate real intent routing in DefaultReasoningPipelineStage
**ID**: #062 *(Legacy: AI.COGNITIVE-003)*
**Status**: 🟡 Medium | #Debt
**Target**: `src/BuildingBlocks/AI.Cognitive/Reasoning/Internal/DefaultReasoningPipelineStage.cs`
**Ref**: DL.04, CS.03

## 📝 Description
Replace the temporary bypass/hardcoded Chat intent in DefaultReasoningPipelineStage with an asynchronous call to _intentNexus.RouteAsync(...) when ready.

## ✅ DoD (Definition of Done)
- [ ] Integrate real intent routing in DefaultReasoningPipelineStage
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests