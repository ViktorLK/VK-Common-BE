# Task: Replace hardcoded Sentiment Analysis with dynamic evaluation
**ID**: LABS.PWP-002
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