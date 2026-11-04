---
id: "#120"
legacy_id: "LABS.PWP-004"
category: debt
parent: null
title: Implement SignalR for Real-time Proactive Push
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
  - 'PWP Phase 4: Full-Duplex Proactive Interaction'
  docs: []
commits: []
---
# Task: Implement SignalR for Real-time Proactive Push
**ID**: #120 *(Legacy: LABS.PWP-004)*
**Status**: 🔴 High | #Debt
**Target**: `VK.Labs.Pwp.Hubs.PwpHub`
**Ref**: PWP Phase 4: Full-Duplex Proactive Interaction

## 📝 Description
Integrate ASP.NET Core SignalR into PWP. Implement PwpHub to support full-duplex communication, enabling the PwpProactiveEngine to push autonomous AI messages and real-time engine state (Pulse) to the client without polling.

## ✅ DoD (Definition of Done)
- [ ] Implement SignalR for Real-time Proactive Push
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests