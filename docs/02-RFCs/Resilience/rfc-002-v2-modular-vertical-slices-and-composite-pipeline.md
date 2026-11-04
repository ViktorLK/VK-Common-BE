# RFC-002: V2 Modular Vertical Slices & Composite Pipeline

- **Status**: 🛑 Superseded by [RFC-003](./rfc-003-v3-thin-contract-pure-delegation-and-polly-v8-engine.md)
- **Author**: Architecture Core Team
- **Date**: 2026-08-14
- **Target Module(s)**: `VK.Blocks.Resilience`
- **Related ADRs**: [ADR-002](/docs/04-ArchitectureDecisionRecords/Resilience/adr-002-modular-vertical-slice-feature-architecture.md), [ADR-003](/docs/04-ArchitectureDecisionRecords/Resilience/adr-003-dynamic-multi-policy-composition-and-http-integration.md), [ADR-004](/docs/04-ArchitectureDecisionRecords/Resilience/adr-004-step-level-composite-resilience-policy.md)

---

## 1. 🎯 Motivation & Problem Statement (背景と課題定義)

V1（RFC-001）の一体型パイプラインでは、「リトライだけをピンポイントで使いたい」「ローカル開発環境では Polly なしで動かしたい」という要求を満たせなかった。
さらに、`AI.Cortex`（LLM ターン制御）や `Workflow`（サガ補償）などの上位ブロックが成長し、ステップ単位でタイムアウトとリトライ回数を柔軟に合成したいという需要が急増した。

## 2. 🧭 Goals & Non-Goals (目標と非目標)

- **Goals**:
  - `VK.Blocks` の **Vertical Slice Architecture（`BB.01`）** に則り、耐障害性を 6 つの独立切片（Retry, Timeout, CircuitBreaker, RateLimiting, Bulkhead, Fallback）に再編。
  - 外部エンジンなしで単体動作する手製 In-Memory デフォルト実装（`Local*`）を同梱（`ARCH-05`）。
  - `VKResult<T>`（`CS.01`）と統合されたエラー定数 `VKResilienceErrors` の整備。
  - ステップ単位の宣言的設定レコード `VKStepResiliencePolicy` および合成ランナー `VKStepResilienceRunner` の導入。
- **Non-Goals (`BASE-06`)**:
  - 分散 Redis と連携したグローバルレートリミッター（ローカルインメモリに限定）。

## 3. 🧬 Metaphor & Mapping (メンタルモデルと構造マッピング)

- **Metaphor (専門安全区画と多重防護隔壁)**:
  巨大な潜水艦の内部を独立した「防水区画（Vertical Slice）」に細分化し、各区画が自律した水密扉（LocalRetry, LocalCircuitBreaker）を持つ構造。司令塔（Cortex / Workflow）は、必要に応じて複数の区画を直列に繋ぎ合わせた「複合防護シールド（VKStepResiliencePolicy）」を動的に展開する。

| Metaphor Component (概念要素) | Technical Artifact (技術要素) | Layer / Scope | Responsibility (責務) |
| :--- | :--- | :--- | :--- |
| 個別防水区画 | `{Feature}/` (Retry, Timeout, etc.) | Vertical Slice (`BB.01`) | 個別防護機能の自己完結切片 |
| 自律式水密扉（ローカル機構） | `LocalRetryExecutor`, `LocalCircuitBreaker` | `Internal/` | In-Memory 自作状態マシン |
| 複合防護シールド指示書 | `VKStepResiliencePolicy` | `Common/Models/` | 不変レコードによる複合ポリシー定義 |
| 統合シールド作動ランナー | `VKStepResilienceRunner` | `Common/` | 各エグゼキュータを順次呼び出す合成ループ |

## 4. 🏗️ Contract & Code Blueprint (契約と主要インターフェース設計)

```csharp
// Level 1 Public API Blueprint (V2)
namespace VK.Blocks.Resilience;

public sealed record VKStepResiliencePolicy
{
    public VKStepRetryPolicy? Retry { get; init; } = VKStepRetryPolicy.Default;
    public VKStepTimeoutPolicy? Timeout { get; init; }
    public VKStepCircuitBreakerPolicy? CircuitBreaker { get; init; }

    public VKStepResiliencePolicy WithRetry(VKStepRetryPolicy? retry) => this with { Retry = retry };
    public VKStepResiliencePolicy WithTimeout(TimeSpan timeout) => this with { Timeout = VKStepTimeoutPolicy.FromTimeSpan(timeout) };
    public VKStepResiliencePolicy WithCircuitBreaker(string key) => this with { CircuitBreaker = VKStepCircuitBreakerPolicy.ForKey(key) };
}
```

## 5. 遷移と歴史的教訓

切片化とインメモリ同梱によってローカルテストの自律性は大幅に向上した。しかし、`LocalCircuitBreaker` や `LocalBulkhead` などの**手製状態マシン・セマフォ管理コードが肥大化（約 1,000 行）**し、スライディングウィンドウの計算誤差やスレッドセーフティの検証コストが増大。これが第 3 波（RFC-003）における「自作コード全廃・薄い契約化と Polly v8 委譲」の契機となった。

