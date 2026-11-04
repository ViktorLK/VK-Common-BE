# RFC-001: V1 Foundation, Resilience Abstractions & Polly Separation

- **Status**: 🛑 Superseded by [RFC-002](./rfc-002-v2-modular-vertical-slices-and-composite-pipeline.md) and [RFC-003](./rfc-003-v3-thin-contract-pure-delegation-and-polly-v8-engine.md)
- **Author**: Architecture Core Team
- **Date**: 2026-04-23
- **Target Module(s)**: `VK.Blocks.Resilience`, `VK.Blocks.Resilience.Polly`
- **Related ADRs**: [ADR-001](/docs/04-ArchitectureDecisionRecords/Resilience/adr-001-decoupling-resilience-contracts-from-engine.md)

---

## 1. 🎯 Motivation & Problem Statement (背景と課題定義)

分散マイクロサービス環境において、下流サービスの一時的な過負荷やネットワーク瞬断が発生した際、即座に例外でリクエストを破滅させるのではなく、自動リトライやタイムアウト、サーキットブレーカーによってシステムを自己回復させる必要がある。
初期のコードベースでは、各サービスやコンポーネントが Polly の `IAsyncPolicy` を直接呼び出していたため、以下の問題が生じていた：
1. **フレームワーク強結合**: Polly の型がドメイン層・アプリケーション層へ漏洩し、ポート＆アダプター原則（`ARCH-02`）が破壊されていた。
2. **構成パラメータの不統一**: リトライ回数、タイムアウト秒数、指数バックオフの係数がサービスごとにバラバラにハードコードされていた。

## 2. 🧭 Goals & Non-Goals (目標と非目標)

- **Goals**:
  - 耐障害性の共通抽象（`IResiliencePipeline`）を定義し、具象エンジン（Polly）からアプリケーションコードを切り離す。
  - 基本的な 4 大戦略（Retry, Timeout, CircuitBreaker, Bulkhead）のインターフェースおよび標準 Options を策定。
- **Non-Goals (`BASE-06`)**:
  - 複雑な動的パイプライン合成やオニオン多段ラッピングの独自実装。
  - 分散レート制限や高度な AI ストリーミング対衝。

## 3. 🧬 Metaphor & Mapping (メンタルモデルと構造マッピング)

- **Metaphor (基礎防火壁と遠隔自動消火弁)**:
  建物（システム）の中に直接スプリンクラーの配管や高圧ポンプ（Polly）を露出させるのではなく、各フロアには標準規格の「消火弁ポート（抽象）」のみを露出させ、実際のポンプ室（Polly エンジン）は物理的に遮断された機械室に隔離する。

| Metaphor Component (概念要素) | Technical Artifact (技術要素) | Layer / Scope | Responsibility (責務) |
| :--- | :--- | :--- | :--- |
| 標準消火弁ポート | `IResiliencePipeline` | Contract Layer | 操作実行の共通プロトコル |
| 各種消火ノズル（リトライ・放水弁） | `IRetryStrategy`, `ITimeoutStrategy` | Contract Layer | 個別防護機能の抽象 |
| 機械室の高圧ポンプ | `PollyResiliencePipeline` | Provider Layer (`Resilience.Polly`) | Polly による具象パイプライン実行 |

## 4. 🏗️ Contract & Code Blueprint (契約と主要インターフェース設計)

```csharp
// Level 1 Public API Blueprint (V1)
namespace VK.Blocks.Resilience;

public interface IResiliencePipeline
{
    Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default);
}

public interface IRetryStrategy
{
    Task<T> ExecuteWithRetryAsync<T>(Func<CancellationToken, Task<T>> action, RetryOptions options, CancellationToken cancellationToken = default);
}
```

## 5. 遷移と歴史的教訓

本 V1 設計は契約とエンジンの分離を確立したものの、インターフェースの粒度が粗く、特定の機能（リトライのみ）を個別に使いたい需要に応えきれなかった。この反省を受け、第 2 波（RFC-002）における垂直切片化（Feature Slice）へと発展した。

