---
id: "#031"
legacy_id: "AI-002"
category: debt
parent: null
title: Configure AI.SemanticKernel Builder
module: AI
type: debt
priority: P1
status: done
sprint: 
estimate: 
created: 2026-05-01
closed: 2026-05-05
refs:
  rules:
  - BB.03, AP.02
  docs: []
commits: []
---
# Task: Configure AI.SemanticKernel Builder
**ID**: #031 *(Legacy: AI-002)*
**Status**: 🔴 High | #Debt
**Target**: `VK.Blocks.AI.SemanticKernel` -> `SemanticKernelBlockRegistration.cs`
**Ref**: BB.03, AP.02

## 📝 Description
Configure builder based on options (OpenAI/AzureOpenAI). Currently contains placeholder logic.

## ✅ DoD (Definition of Done)
- [ ] Implement conditional builder configuration
- [ ] Add support for Azure OpenAI options
- [ ] Verify with integration tests
