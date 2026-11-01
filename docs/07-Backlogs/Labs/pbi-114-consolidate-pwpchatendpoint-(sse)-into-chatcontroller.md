---
id: "#114"
legacy_id: "LABS.PERSONAWEAVEPULSAR-001"
category: debt
parent: null
title: Consolidate PwpChatEndpoint (SSE) into ChatController
module: Labs
type: debt
priority: P2
status: icebox
sprint: 
estimate: 
created: 2026-05-01
closed: 
refs:
  rules: []
  docs: []
commits: []
---
# Task: Consolidate PwpChatEndpoint (SSE) into ChatController
**ID**: #114 *(Legacy: LABS.PERSONAWEAVEPULSAR-001)*
**Status**: 🟡 Medium | #Debt
**Target**: `PwpChatEndpoint.cs, ChatController.cs`
**Ref**: N/A

## 📝 Description
Move the Minimal API SSE streaming logic from `PwpChatEndpoint.cs` into a new `[HttpPost("stream")]` action inside `ChatController.cs`. After migration, delete `PwpChatEndpoint.cs` to unify the API layer strictly into standard MVC Controllers.

## ✅ DoD (Definition of Done)
- [ ] Consolidate PwpChatEndpoint (SSE) into ChatController
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests