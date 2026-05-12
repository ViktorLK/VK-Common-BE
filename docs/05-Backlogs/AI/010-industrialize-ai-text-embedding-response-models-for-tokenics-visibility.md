# Task: Industrialize AI Text/Embedding Response Models for Tokenics Visibility
**ID**: AI-010
**Status**: 🟡 Medium | #Debt
**Target**: `IVKTextEngine, IVKEmbeddingEngine`
**Ref**: DL.04, PS.01

## 📝 Description
Refactor IVKTextEngine and IVKEmbeddingEngine to return structured response objects (e.g., VKTextResponse, VKEmbeddingResponse) instead of raw strings/vectors. This is required to carry metadata like VKTokenUsage in the response without modifying the Core VKResult contract, ensuring consistency with the Chat module's real-time tokenics capability.

## ✅ DoD (Definition of Done)
- [ ] Industrialize AI Text/Embedding Response Models for Tokenics Visibility
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests