# ADR-005: Transition to Thin Contract and Polly v8 Engine Architecture

- **Status**: ✅ Accepted
- **Date**: 2026-10-30
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/Resilience, src/BuildingBlocks/Resilience.Polly
- **Supersedes**: [ADR-001](./adr-001-decoupling-resilience-contracts-from-engine.md), [ADR-002](./adr-002-modular-vertical-slice-feature-architecture.md), [ADR-003](./adr-003-dynamic-multi-policy-composition-and-http-integration.md)

---

## 1. Context (背景)

VK.Blocks の耐障害性（Resilience）機構において、従来（ADR-002, ADR-003）は `VK.Blocks.Resilience` 内に自作のセマフォ（`LocalBulkhead`）、自作状態マシン（`LocalCircuitBreaker`）、自作トークンバケット（`LocalRateLimiter`）、および自作リトライループ（`LocalRetryExecutor` / `VKStepResilienceRunner`）が混在していた。

これにより以下の問題が発生していた：
1. **車輪の再発明と保守リスク（BASE-06 違反）**: 産業標準である Polly v8（`Microsoft.Extensions.Resilience`）が高信頼・高並行・ゼロアロケーションな耐障害性パイプラインを提供しているにもかかわらず、手製の不完全な状態管理を抱え込んでいた。
2. **具象インフラの漏洩と結合（ARCH-02, CS.02 違反）**: Application レイヤー（`AI.Cortex`, `Workflow`）が具象的なリトライやサーキットブレーカーの個別インターフェースに直接依存していた。また、`Authentication.StackExchangeRedis` など無関係なブロックが `Resilience.Polly` を直接プロジェクト参照するインフラ漏洩が発生していた。
3. **VKResult と例外制御流の不整合（BASE-05 違反）**: Polly などの外部エンジンは通常例外（Exception）の発生をトリガーとするが、VK.Blocks では業務的・一過性の失敗は `VKResult<T>` で表現する。この橋渡しが未整備であった。

## 2. Problem Statement (問題定義)

1. **手製状態マシンの限界**: 自作の `LocalCircuitBreaker` や `LocalBulkhead` は、指数バックオフや Jitter、スライディングウィンドウによる正確な障害率計算において、本番高並行負荷に耐えうる検証が不十分であった。
2. **多面的なインターフェースの氾濫**: 呼び出し元が `IVKRetryExecutor`, `IVKTimeoutExecutor`, `IVKCircuitBreaker` をそれぞれ個別に注入する必要があり、DI 構成が肥大化していた。
3. **ValueTask 誤用リスク（CS.03）**: 公開インターフェースにおいて `ValueTask<T>` を返却すると、呼び出し側での二重 await やライフサイクル破綻のリスクを孕んでいた。

## 3. Decision (決定事項)

`VK.Blocks.Resilience` を **薄い契約（Thin Contract）** に純化し、`VK.Blocks.Resilience.Polly` を **唯一の実行エンジン** として位置付ける。

1. **統一実行ポート `IVKResilienceExecutor` の策定（`ARCH-02`, `CS.03`）**:
   - Level 1 公開 API として、戻り値に `Task<T>` を強制（`CS.03`）。
   - `ExecuteAsync`（Action / ValueTask）および `ExecuteResultAsync`（`VKResult<T>` の結果感知）を提供。
2. **デフォルト NoOp 実装の同梱（`ARCH-05`, `AP.02`）**:
   - `NoOpResilienceExecutor` を Fallback Tier として契約パッケージ内に同梱（`TryAddSingleton`）。外部エンジンが未導入でも安全にパススルー実行。
3. **Polly v8 による決定論的オーバーライド（`ARCH-05`, `AP.06`）**:
   - `VK.Blocks.Resilience.Polly` が `PollyResilienceExecutor` を Provided Tier として提供。`services.Replace()` により決定論的に上書き。
4. **Adapter 内部閉環例外ブリッジパターン（`BASE-05`, `CS.01`）**:
   - 内部例外 `VKResilienceTransientFailureException` を定義。
   - `ExecuteResultAsync` 内部で `VKResult<T>` の一過性エラー（`ServiceUnavailable`, `TooManyRequests`, `ExternalError`, `Timeout`）を検知した際、Adapter 境界内でのみ本例外を送出して Polly の非汎型パイプラインに補足を促す。
   - Polly のパイプライン完了時に本例外を再捕獲し、呼び出し元へ純粋な `VKResult<T>` として返却。Application レイヤーの例外フリー原則（`BASE-05`）を厳格に保持。
5. **固定パイプラインスタッキング順序（`OR.03`）**:
   - `Total Timeout` ➔ `[Hedging]` ➔ `Retry` ➔ `Circuit Breaker` ➔ `[RateLimiter / Concurrency]` ➔ `Attempt Timeout` の順序を統一。
6. **TimeProvider の統一活用（`CS.06`, `BASE-02`）**:
   - `VK.Blocks.Core` の `TimeProvider.System` を Polly パイプラインに自動配線。テスト時は `FakeTimeProvider` による決定論的仮想時間検証を可能とする。
7. **横断的インフラ依存の除去（`ARCH-01`, `CS.02`）**:
   - `Authentication.StackExchangeRedis.csproj` から不要な `Resilience.Polly` 参照を完全削除。

## 4. Alternatives Considered (代替案の検討)

- **Option 1: 自作状態マシン（Local*）の維持・機能追加**:
  - *Approach*: 自作の `LocalCircuitBreaker` 等にスライディングウィンドウや Jitter を追加実装。
  - *Rejected Reason*: 産業標準である Polly v8 の巨大な投資を無視して独自に並行性バグのリスクを負うことは、[BASE-06] Radical YAGNI に真っ向から反するため却下。
- **Option 2: 契約層に直接 Polly パッケージを参照させる**:
  - *Approach*: `VK.Blocks.Resilience` に直接 Polly を NuGet 参照させ、1パッケージ化。
  - *Rejected Reason*: アプリケーション層や他ブロック（`AI.Cortex`, `Workflow`）が特定のサードパーティ耐障害性ライブラリに拘束され、ポート＆アダプター原則（[ARCH-02]）に違反するため却下。

## 5. Consequences & Mitigation (結果と緩和策)

- **Positive**:
  - 自作コード約 1,000 行を完全排出し、Polly v8 の最高峰のパフォーマンス、ゼロアロケーション（`CS.04`）、および堅牢性を獲得。
  - `IVKResilienceExecutor` への一本化により、呼び出し元の依存性が劇的にシンプル化。
  - OpenTelemetry への標準メトリクス（`Polly` メーター）出力が追加コードなしで自動連動。
- **Negative / Mitigation**:
  - レガシーな個別インターフェース（`IVKRetryExecutor` 等）を使用している既存モジュールが存在するが、後方互換ブリッジ（`VKStepResilienceRunner` のオーバーロード等）を用意することで、段階的移行を可能とした。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- すべての Polly 具体型（`PollyResilienceExecutor`, `PollyStrategyMapper` 等）は `internal sealed` に隠蔽され、外部への型漏洩を完全防止（`ARCH-07`, `AP.03`）。
- 秘密情報（API キー、Bearer トークン、接続文字列）は、トレーススパンや例外メッセージに一切含めない（`FS-12`, `OR.02`）。

