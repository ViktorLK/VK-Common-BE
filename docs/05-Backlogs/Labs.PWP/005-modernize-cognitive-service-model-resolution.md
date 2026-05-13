# Task: Modernize Cognitive Service Model Resolution
**ID**: LABS.PWP-005
**Status**: 🟡 Medium | #Debt
**Target**: `AICognitiveSKBlockRegistration.cs`
**Ref**: PWP-004

## 📝 Description
Refactor all service registrations in AICognitiveSKBlockRegistration to resolve ModelId dynamically via IVKChatOptionsProvider instead of relying on static VKAISKOptions.DeploymentName. This fixes the 'Unknown' fallback and ensures correct metrics/logging when using non-Azure providers (Google, OpenAI, Ollama).

## ✅ DoD (Definition of Done)
- [ ] Modernize Cognitive Service Model Resolution
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests