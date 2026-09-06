# Task: Support Streaming Pipeline (IVKPsycheStreamingPipeline & TTFT Profiling)
**ID**: AI.PSYCHE-001
**Status**: 🔴 High | #Debt
**Target**: `src/BuildingBlocks/AI.Psyche/Pipeline`
**Ref**: BB.01, CS.03

## 📝 Description
Establish the streaming execution pipeline in AI.Psyche by introducing IVKPsycheStreamingPipeline (or ExecuteStreamingAsync), connecting to IVKChatEngine.SendStreamingAsync, and integrating stream-level lifecycle events (TTFT profiling, Echo conversation accumulation, and stream cancellation).

## ✅ DoD (Definition of Done)
- [ ] Support Streaming Pipeline (IVKPsycheStreamingPipeline & TTFT Profiling)
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests