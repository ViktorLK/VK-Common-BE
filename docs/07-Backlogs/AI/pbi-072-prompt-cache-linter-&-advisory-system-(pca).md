---
id: "#072"
legacy_id: "AI.PSYCHE-006"
category: debt
parent: null
title: Prompt Cache Linter & Advisory System (PCA)
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
  - AP.07
  - DL.04
  - BB.04
  docs: []
commits: []
---
# Task: Prompt Cache Linter & Advisory System (PCA)
**ID**: #072 *(Legacy: AI.PSYCHE-006)*
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