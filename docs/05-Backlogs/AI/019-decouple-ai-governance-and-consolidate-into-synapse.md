# Task: Decouple AI Governance Abstractions and Consolidate Traffic Management into AI.Synapse
**ID**: BUILDINGBLOCKS.AI-019
**Status**: 🟡 Medium | #Refactor #Architecture
**Target**: `src/BuildingBlocks/AI/Governance`, `src/BuildingBlocks/AI.SemanticKernel`, `src/BuildingBlocks/AI.Synapse`
**Ref**: DL.04 / BB.01 / BB.06

## 📝 Description
针对 `VK.Blocks.AI` 基础库中的早期 Governance 体系（`IVKAIGovernanceOptions` 及相关孤立协议）与 `VK.Blocks.AI.Synapse` 之间的职责重叠进行系统性解耦重构：
- **清理早期孤立协议**：移除 `AI/Governance/Protocols` 中未实现的接口（如 `IVKAICostEvaluator`、`IVKAISemanticCache`、`IVKAIGuardrail`、`IVKAIPIIMasker` 等）。
- **统一流量治理职责**：确保流量路由、多租户连接池、成本算费（`IVKAICostCalculator`）与配额限流（`IAITokenBudgetManager`、`IVKAIProviderTracker`）全面归集到 `AI.Synapse`。
- **重构 Options 继承约束**：评估并解除 `AI.SemanticKernel` 中 `AISemanticKernelEngineBase<TOptions>` 对 `IVKAIGovernanceOptions` 的强泛型约束，使其平滑迁移到纯粹的 `IVKAIProviderOptions` + `IVKToggleableBlockOptions`。
- **精简 `VKAIOptions`**：移除基础库 `VKAIOptions` 中未被消费的重试与熔断配置字段（`RetryCount`, `Timeout`, `CircuitBreakerThreshold` 等）。

## ✅ DoD (Definition of Done)
- [ ] 移除 `AI/Governance` 下无实际实现的占位协议与冗余 Settings。
- [ ] 重构 `AI.SemanticKernel` 引擎基类泛型约束，解除对 `IVKAIGovernanceOptions` 的依赖。
- [ ] 确保全量 `AI` 各切片 Options 保持轻量纯粹。
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] 全量编译与单元测试通过。
