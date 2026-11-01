---
id: "#117"
legacy_id: "LABS.PWP-002"
category: debt
parent: null
title: Replace hardcoded Sentiment Analysis with dynamic evaluation
module: Labs
type: debt
priority: P1
status: todo
sprint: 
estimate: 
created: 2026-05-01
closed: 
refs:
  rules:
  - 'PWP Phase 3: Emotional Feedback Loop'
  docs: []
commits: []
---
# Task: Replace hardcoded Sentiment Analysis with dynamic evaluation
**ID**: #117 *(Legacy: LABS.PWP-002)*
**Status**: 🔴 High | #Debt
**Target**: `VK.Labs.Pwp.Monitoring.PwpSelfMonitor`
**Ref**: PWP Phase 3: Emotional Feedback Loop

## 📝 Description
Replace the hardcoded 0.5 UserSentiment in PwpChatEndpoint with a real analysis phase. Use either a lightweight LLM call or a sentiment analysis library to extract emotional tone from user inputs.

## ✅ DoD (Definition of Done)
- [ ] Replace hardcoded Sentiment Analysis with dynamic evaluation
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests