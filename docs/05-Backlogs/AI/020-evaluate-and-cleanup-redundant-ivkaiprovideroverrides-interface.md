# Task: Evaluate and Cleanup Redundant IVKAIProviderOverrides Interface
**ID**: AI-020
**Status**: 🟡 Medium | #Debt
**Target**: `src/BuildingBlocks/AI/Common/Connection/Protocols/IVKAIProviderOverrides.cs`
**Ref**: AP.05 (Args Pattern & SG Automation)

## 📝 Description
Evaluate and eliminate the redundant IVKAIProviderOverrides interface in AI/Common/Connection. Since connection override properties (Provider, ModelId, ApiKey, Endpoint) are now directly and automatically generated on concrete *Args records (such as VKChatArgs) by the Source Generator, this manual interface has become redundant legacy abstraction and can be safely retired.

## ✅ DoD (Definition of Done)
- [ ] Evaluate and Cleanup Redundant IVKAIProviderOverrides Interface
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests