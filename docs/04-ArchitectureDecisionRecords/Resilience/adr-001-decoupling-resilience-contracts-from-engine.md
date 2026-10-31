# ADR-001: Decoupling Resilience Strategy Contracts from Implementation Engines

- **Status**: ❌ Superseded by [ADR-002](./adr-002-modular-vertical-slice-feature-architecture.md) and [ADR-005](./adr-005-transition-to-thin-contract-and-polly-v8-engine.md)
- **Date**: 2026-04-23
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/Resilience

---

## 1. Context (背景)

分散マイクロサービス環境および外部 SaaS/クラウド SDK との連携において、一時的なネットワーク障害、サービス瞬断、および高負荷によるレイテンシ急増に対する回復力（Resilience）の確保は必須要件である。
初期の VK.Blocks では、回復力処理を各マイクロサービスが独自に Polly 等のサードパーティライブラリを直接呼び出して記述していたため、ポリシー定義の重複やリトライ・タイムアウトのパラメータ不統一が発生していた。

## 2. Problem Statement (問題定義)

各サービスやモジュールが Polly や外部ライブラリに直接依存することには以下の重大な課題があった：
1. **ライブラリ結合とバージョン拘束**: Polly のメジャーバージョンアップ（v7 から v8 への破壊的変更等）が全マイクロサービスのコードに波及する。
2. **Clean Architecture / ドメイン純粋性の侵害**: ドメイン層やアプリケーション層に Polly 固有の型（`IAsyncPolicy`, `PolicyWrap`）が漏洩し、ポート＆アダプター原則に違反する。
3. **テストの困難性**: 外部耐障害性ライブラリが密結合していると、単体テストでタイムアウトやリトライの振る舞いをモック・シミュレーションすることが極めて困難となる。

## 3. Decision (決定事項)

耐障害性の共通インターフェースを定義する中立な抽象ブロック **`VK.Blocks.Resilience`** と、Polly による具象実装を提供する **`VK.Blocks.Resilience.Polly`** を分離して設計する。

1. **インターフェースによる抽象化**:
   - `IResiliencePipeline` および `IResiliencePipelineBuilder` を定義し、パイプラインの構築と実行を抽象化する。
   - `IRetryStrategy`, `ICircuitBreakerStrategy`, `ITimeoutStrategy`, `IBulkheadStrategy` の個別戦略インターフェースを定義。
2. **コンテキストの受け渡し**:
   - 実行時のメタデータ（相関ID、試行回数等）をカプセル化する `ResilienceContext` を導入。
3. **設定オプションの標準化**:
   - `RetryOptions`, `CircuitBreakerOptions`, `TimeoutOptions`, `BulkheadOptions` を定義し、設定ファイルからのバインドを標準化。

## 4. Alternatives Considered (代替案の検討)

- **Option 1: 全サービスで Polly を直接利用**:
  - *Approach*: 共通抽象を作らず、Polly を直接 NuGet 参照して利用。
  - *Rejected Reason*: 具象インフラへの強結合が発生し、ドメイン層の汚染および将来のエンジン置換が不可能となるため却下。
- **Option 2: 共通モジュール内に Polly を内包した一体型パッケージ**:
  - *Approach*: `VK.Blocks.Resilience` に Polly 依存を含め、1パッケージで提供。
  - *Rejected Reason*: 契約（ポート）と実装（アダプター）の物理的分離原則（`ARCH-02`）に反し、軽量な契約のみを参照したいモジュールにも不要なバイナリ依存が強制されるため却下。

## 5. Consequences & Mitigation (結果と緩和策)

- **Positive**:
  - アプリケーションコードから外部ライブラリ依存が排除され、ポータビリティが向上。
  - 設定の一元管理と耐障害性パラメータの標準化が実現。
- **Negative**:
  - ラッパー層を介することによるわずかな抽象化オーバーヘッド。
  - 当初設計のインターフェース群（`IResiliencePipeline` 等）が粒度的に粗く、垂直切片アーキテクチャへの統合に課題を残した（ADR-002 にて改訂）。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- 初期実装として `VK.Blocks.Resilience` に抽象型と Options を配置し、`VK.Blocks.Resilience.Polly` に `PollyPipelineBuilder` および各戦略の実装を配置。
- 機微情報（API トークンや認証ヘッダー）が `ResilienceContext` やログに漏洩しないよう配慮。

