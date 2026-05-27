# Task: Integrate real intent routing in DefaultReasoningPipelineStage
**ID**: AI.COGNITIVE-002
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