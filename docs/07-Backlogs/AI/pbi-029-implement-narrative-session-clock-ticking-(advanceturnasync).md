---
id: "#029"
legacy_id: "AI.COGNITIVE-001"
category: debt
parent: null
title: Implement Narrative Session Clock Ticking (AdvanceTurnAsync)
module: AI
type: debt
priority: P2
status: done
sprint: 
estimate: 
created: 2026-05-01
closed: 2026-05-05
refs:
  rules: []
  docs: []
commits: []
---
# Task: Implement Narrative Session Clock Ticking (AdvanceTurnAsync)
**ID**: #029 *(Legacy: AI.COGNITIVE-001)*
**Status**: ✅ Done
**Target**: `N/A`
**Ref**: N/A

## 📝 Description
Define and implement turn-timer time-flow mechanism inside IVKKnowledgeSessionStateStore and BasicKnowledgeNarrativeManager to decrement CooldownRemainingTurns, StickyRemainingTurns, and AdvanceTurn counts.

## ✅ DoD (Definition of Done)
- [x] Implement Narrative Session Clock Ticking (AdvanceTurnAsync)
- [x] **Assess if an ADR is required (DL.03)**
- [x] Verify changes
- [x] Run tests