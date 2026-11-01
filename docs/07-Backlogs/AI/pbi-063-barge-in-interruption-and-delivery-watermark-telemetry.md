---
id: "#063"
legacy_id: "AI.EFFERENT-004"
category: debt
parent: null
title: Barge-in Interruption and Delivery Watermark Telemetry
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
  - ai-efferent-manifest Section 10
  docs: []
commits: []
---
# Task: Barge-in Interruption and Delivery Watermark Telemetry
**ID**: #063 *(Legacy: AI.EFFERENT-004)*
**Status**: 🟡 Medium | #Debt
**Target**: `BuildingBlocks/AI.Efferent/EgressAudio`
**Ref**: ai-efferent-manifest Section 10

## 📝 Description
Implement barge-in interruption signal handling to immediately terminate ongoing TTS audio and token streams, and calculate/report actual delivery watermark position back to AI.Psyche/AI.Engram.

## ✅ DoD (Definition of Done)
- [ ] Barge-in Interruption and Delivery Watermark Telemetry
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests