# Task: Prompt Cache Linter & Advisory System (PCA)
**ID**: AI.PSYCHE-006
**Status**: 🟡 Medium | #Debt
**Target**: `src/BuildingBlocks/AI.Psyche/Weaving/`
**Ref**: AP.07, DL.04, BB.04

## 📝 Description
Design and implement a non-intrusive Prompt Cache Advisor (PCA) for AI.Psyche. It inspects prompt segment volatility and variable placements during the weaving stage without imposing hard restrictions (AP.07). Emits structured warnings (LogWarning, OTel tag 'ai.psyche.cache.warning', and Response Metadata) when volatile variables (e.g. CurrentTime, RequestId) or per-turn RAG dynamic chunks are placed before static/warm prefixes, breaking implicit prefix caching for LLM providers (OpenAI, DeepSeek, Anthropic).

## ✅ DoD (Definition of Done)
- [ ] Prompt Cache Linter & Advisory System (PCA)
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests