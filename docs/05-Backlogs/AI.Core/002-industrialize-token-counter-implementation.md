# Task: Industrialize Token Counter Implementation
**ID**: AI.CORE-002
**Status**: 🔴 High | #Debt
**Target**: `VK.Blocks.AI.Internal.VKTokenCounter`
**Ref**: CS.04: Performance & Memory

## 📝 Description
Replace the placeholder logic in VKTokenCounter with a production-grade tokenizer (e.g. Tiktoken for OpenAI/Azure). This ensures accurate context window management and prevents 'Token limit exceeded' errors.

## ✅ DoD (Definition of Done)
- [ ] Industrialize Token Counter Implementation
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests