---
id: "#070"
legacy_id: "AI.SEMANTICKERNEL-005"
category: debt
parent: null
title: Refactor AI.SemanticKernel registration using VKBlockBuilderExtensions syntax sugar
module: AI
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
# Task: Refactor AI.SemanticKernel registration using VKBlockBuilderExtensions syntax sugar
**ID**: #070 *(Legacy: AI.SEMANTICKERNEL-005)*
**Status**: 🟡 Medium | #Debt
**Target**: `N/A`
**Ref**: N/A

## 📝 Description
利用 VKBlockBuilderExtensions 提供的 WithScoped / WithSingleton 等语法糖重构 AI.SemanticKernel 及相关 AI 适配器模块的注册机制，替代底层 services.Replace，实现声明式覆写与插件挂载。

## ✅ DoD (Definition of Done)
- [ ] Refactor AI.SemanticKernel registration using VKBlockBuilderExtensions syntax sugar
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests