# Task: Consolidate PwpChatEndpoint (SSE) into ChatController
**ID**: LABS.PERSONAWEAVEPULSAR-001
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