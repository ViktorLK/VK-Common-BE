# Task: Implement Channels-based asynchronous pipeline post-processing and backpressure
**ID**: AI.PSYCHE-003
**Status**: 🟡 Medium | #Debt
**Target**: `src/BuildingBlocks/AI.Psyche/Pipeline`
**Ref**: CS.03, AP.07

## 📝 Description
Introduce System.Threading.Channels to decouple high-priority LLM stream consumption from background After-phase tasks (Echo persistence and Session metrics update), providing smooth backpressure and non-blocking streaming experience.

## ✅ DoD (Definition of Done)
- [ ] Implement Channels-based asynchronous pipeline post-processing and backpressure
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests