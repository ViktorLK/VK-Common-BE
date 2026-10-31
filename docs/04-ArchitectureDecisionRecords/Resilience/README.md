# Architecture Decision Records (ADR) - Resilience

このディレクトリには、`VK.Blocks.Resilience`（耐障害性契約）および `VK.Blocks.Resilience.Polly`（実行エンジン）における主要なアーキテクチャ意思決定の履歴が記録されています。

---

## 📊 意思決定タイムライン (Decision Timeline)

```text
2026-04-23: [ADR-001] 契約と具象 Polly エンジンの分離（初期設計）❌ Superseded
      │
2026-08-14: [ADR-002] 垂直切片（Feature Slice）化と In-Memory デフォルト同梱 ❌ Superseded
      │
2026-08-25: [ADR-003] 動的マルチポリシー合成パイプラインと HTTP DelegatingHandler ❌ Superseded
      │
2026-08-27: [ADR-004] AI / ワークフロー向けステップレベル複合ポリシー (VKStepResiliencePolicy) ✅ Accepted
      │
2026-10-30: [ADR-005] 薄い契約 (IVKResilienceExecutor) 化と Polly v8 唯一エンジン統合 ✅ Accepted
```

---

## 📚 ADR 一覧 (ADR Index)

### 1. 最新アクティブ決定事項 (Active Decisions)

#### [ADR-005: Transition to Thin Contract and Polly v8 Engine Architecture](./adr-005-transition-to-thin-contract-and-polly-v8-engine.md)
- **Status**: ✅ Accepted
- **Date**: 2026-10-30
- **概要**: 自作の状態マシンやリトライループを完全排出し、`IVKResilienceExecutor` を中核とする「薄い契約」へと純化。Polly v8（`Microsoft.Extensions.Resilience`）を唯一の実行エンジンとして統合し、Adapter 内部閉環例外ブリッジにより `VKResult<T>` の結果感知（無例外制御流）を実現する。
- **キーワード**: Thin Contract, Polly v8, Port-and-Adapter, Clean Adapter Bridge, Zero-Allocation, TimeProvider

#### [ADR-004: Step-Level Composite Resilience Policy for AI and Workflow Orchestration](./adr-004-step-level-composite-resilience-policy.md)
- **Status**: ✅ Accepted
- **Date**: 2026-08-27
- **概要**: `AI.Cortex`（LLM ターン制御）や `Workflow`（サガ補償）におけるステップ実行のため、不変レコード `VKStepResiliencePolicy` を導入し、ステップ固有の耐障害性設定を宣言的にバインド・オーバーライド可能にする。
- **キーワード**: Step-Level Policy, AI.Cortex, Workflow, Immutability, Declarative Configuration

---

### 2. 過去の改定履歴 (Superseded History)

#### [ADR-003: Dynamic Multi-Policy Composition Pipeline and HTTP Integration](./adr-003-dynamic-multi-policy-composition-and-http-integration.md)
- **Status**: ❌ Superseded by [ADR-005](./adr-005-transition-to-thin-contract-and-polly-v8-engine.md)
- **Date**: 2026-08-25
- **概要**: 複数のポリシーを動的に合成する `IVKPolicyBuilder` / `IVKPolicyRegistry` および HTTP クライアント向けの `VKResilienceDelegatingHandler` を自作実装。
- **キーワード**: Composite Pipeline, PolicyBuilder, DelegatingHandler, TokenBucket

#### [ADR-002: Modular Vertical Slice Feature Architecture with In-Memory Defaults](./adr-002-modular-vertical-slice-feature-architecture.md)
- **Status**: ❌ Superseded by [ADR-005](./adr-005-transition-to-thin-contract-and-polly-v8-engine.md)
- **Date**: 2026-08-14
- **概要**: モジュールを垂直切片（Bulkhead, CircuitBreaker, Retry, Timeout 等）に分割し、Source Generator Feature マーカー（`[VKFeature]`）と手製の In-Memory 実装（`Local*`）を同梱。
- **キーワード**: Vertical Slice Architecture, Feature Marker, In-Memory Defaults, Result Pattern

#### [ADR-001: Decoupling Resilience Strategy Contracts from Implementation Engines](./adr-001-decoupling-resilience-contracts-from-engine.md)
- **Status**: ❌ Superseded by [ADR-002](./adr-002-modular-vertical-slice-feature-architecture.md) and [ADR-005](./adr-005-transition-to-thin-contract-and-polly-v8-engine.md)
- **Date**: 2026-04-23
- **概要**: アプリケーションコードから Polly への直接依存を排除するため、中立な契約パッケージ `VK.Blocks.Resilience` とプロバイダーパッケージ `VK.Blocks.Resilience.Polly` を分離設計。
- **キーワード**: Engine Decoupling, Port-and-Adapter, Abstraction Layer

