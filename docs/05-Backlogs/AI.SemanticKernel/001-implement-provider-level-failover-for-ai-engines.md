# Task: Implement Provider-Level Failover for AI Engines
**ID**: AI.SEMANTICKERNEL-001
**Status**: 🟡 Medium | #Debt
**Target**: `AISKChatEngine.cs, VKAISKEngineBase.cs`
**Ref**: OR.03, Conversation: eee87fe8-44ba-4fa7-b176-cbc8cef7a6c9

## 📝 Description
Currently, AI engines only have transport-level resilience (retries via HttpClient). Implement higher-level failover logic in AISKChatEngine and other engines to switch between providers (e.g., from OpenAI to AzureOpenAI) when a primary provider returns consistent failures or is unavailable. This should follow the VK.Blocks resiliency patterns.

## ✅ DoD (Definition of Done)
- [ ] Implement Provider-Level Failover for AI Engines
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests