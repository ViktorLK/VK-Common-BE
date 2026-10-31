# ADR-003: Dynamic Multi-Policy Composition Pipeline and HTTP Integration

- **Status**: ❌ Superseded by [ADR-005](./adr-005-transition-to-thin-contract-and-polly-v8-engine.md)
- **Date**: 2026-08-25
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/Resilience
- **Supersedes**: [ADR-002](./adr-002-modular-vertical-slice-feature-architecture.md)

---

## 1. Context (背景)

垂直切片化（ADR-002）により個別ストラテジーの利用は容易になったが、実際の運用現場（外部 API 呼び出しや HTTP クライアント）では、「タイムアウト ➔ リトライ ➔ サーキットブレーカー ➔ レート制限」を組み合わせた **複合パイプライン（Composite Pipeline）** の実行が求められていた。
また、`HttpClient` に対する透過的な耐障害性付与（DelegatingHandler）や、テスト容易性のための `TimeProvider` 統一注入（`CS.06`）への適合が不可欠となっていた。

## 2. Problem Statement (問題定義)

1. **個別エグゼキュータの手動ネスト**: 複合ポリシーを適用する際、呼び出し側が `IVKTimeoutExecutor` の中で `IVKRetryExecutor` を呼び出すような多段コールバックネストを記述する必要があり、認知負荷が高かった。
2. **ポリシーの事前登録と名前引きの欠如**: 異なる外部サービス宛先（例: OpenAI 用、社内 API 用）ごとに異なるポリシー組み合わせを名前付きで管理する仕組みがなかった。
3. **HTTP 統合の欠如**: `IHttpClientBuilder` に対する標準拡張がなく、ASP.NET Core の HTTP クライアントファクトリとの統合が手作業となっていた。
4. **決定論的テストの不足**: リトライのバックオフ遅延や断路器のサンプリング窓において `DateTime.UtcNow` や `Task.Delay` が直接呼ばれる箇所があり、テスト実行時間増大の要因となっていた。

## 3. Decision (決定事項)

複数のポリシーを流れるように合成できる **ポリシービルダー・レジストリ機構** と **HTTP 透過ハンドラー** を導入する。

1. **複合パイプライン設計（`ARCH-11`）**:
   - `IVKPolicyRegistry` および `DefaultPolicyRegistry` を新設し、名前付きパイプラインの登録・取得を一元化。
   - `IVKPolicyBuilder` および `DefaultPolicyBuilder` を導入し、Fluent API でポリシーの合成を宣言可能化。
   - `DefaultResiliencePipeline` が登録されたポリシー群をオニオン状に合成実行。
2. **HTTP DelegatingHandler 統合（`OR.03`）**:
   - `VKResilienceDelegatingHandler` を実装し、送信 HTTP メッセージに対してパイプラインを自動適用。
   - `VKHttpClientResilienceExtensions.AddVKResilienceHandler()` を提供。
3. **`TimeProvider` の全面注入（`CS.06`）**:
   - `LocalRetryExecutor`, `LocalTimeoutExecutor`, `LocalCircuitBreaker` に `TimeProvider` をインジェクションし、仮想時間による即時テストに対応。
4. **トークンバケットアルゴリズムの正式採用**:
   - レート制限切片にバースト耐性を持つ `LocalTokenBucketLimiter`（`IVKTokenBucketLimiter`）を追加。

## 4. Alternatives Considered (代替案の検討)

- **Option 1: Polly の PolicyWrap を直接利用**:
  - *Approach*: 複合パイプラインの合成のみ Polly の `PolicyWrap` に委託。
  - *Rejected Reason*: 契約パッケージ（`VK.Blocks.Resilience`）に Polly 依存が混入し、契約とエンジンの分離原則（`ARCH-02`）が崩れるため却下。
- **Option 2: 呼び出し側での手動合成を継続**:
  - *Approach*: ライブラリは個別エグゼキュータのみを提供し続け、合成は各アプリケーションに委ねる。
  - *Rejected Reason*: アプリケーション層にボイラープレートコードが蔓延し、ポリシー順序の不統一による障害（例: タイムアウトをリトライの外側に置くべきか内側に置くべきかの混乱）が多発するため却下。

## 5. Consequences & Mitigation (結果と緩和策)

- **Positive**:
  - `DefaultPolicyBuilder` による直感的なパイプライン合成と `HttpClient` 統合が実現。
  - `TimeProvider` 導入により、`FakeTimeProvider` を使った決定論的テストが可能となった。
- **Negative**:
  - 自作の `DefaultResiliencePipeline` および個別ポリシーラッパー（`RetryResiliencePolicy` 等）が増加し、ライブラリ内部のクラス数が肥大化。
  - 後にマイクロソフト公式の Polly v8 / `Microsoft.Extensions.Resilience` が同等以上の合成機能を提供したため、本設計は ADR-005 にて刷新された。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- `VKResilienceHealthCheck` を追加し、登録されている断路器の稼働状態（Open/Closed/HalfOpen）を ASP.NET Core HealthChecks 経由で監視可能化。
- 各ポリシー実行時に `ActivitySource`（`VKResilienceDiagnostics`）による分散トレーススパンを記録。

