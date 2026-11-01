---
id: "#055"
legacy_id: "AI.CORE-002"
category: debt
parent: null
title: Industrialize Token Counter Implementation
module: AI
type: debt
priority: P1
status: todo
sprint: 
estimate: 
created: 2026-05-01
closed: 
refs:
  rules:
  - 'CS.04: Performance & Memory'
  docs: []
commits: []
---
# Task: Industrialize Token Counter Implementation
**ID**: #055 *(Legacy: AI.CORE-002)*
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