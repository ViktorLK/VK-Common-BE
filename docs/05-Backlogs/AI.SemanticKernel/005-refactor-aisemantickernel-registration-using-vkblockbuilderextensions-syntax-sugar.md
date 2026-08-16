# Task: Refactor AI.SemanticKernel registration using VKBlockBuilderExtensions syntax sugar
**ID**: AI.SEMANTICKERNEL-005
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