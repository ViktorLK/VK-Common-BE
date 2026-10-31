# ADR-002: Modular Vertical Slice Feature Architecture with In-Memory Defaults

- **Status**: ❌ Superseded by [ADR-005](./adr-005-transition-to-thin-contract-and-polly-v8-engine.md)
- **Date**: 2026-08-14
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/Resilience
- **Supersedes**: [ADR-001](./adr-001-decoupling-resilience-contracts-from-engine.md)

---

## 1. Context (背景)

VK.Blocks 全体における **Vertical Slice Architecture（`BB.01`）** および **Source Generator（SG）駆動アーキテクチャ（`BB.02`〜`BB.08`）** の標準化推進に伴い、従来の粗粒度なパイプライン設計（ADR-001）を再検討する必要が生じた。
特に、単一の耐障害性機能（例: リトライのみ、またはタイムアウトのみ）を個別に利用したいケースや、外部エンジン（Polly）を導入せずともローカル開発やテスト環境で即座に動作する軽量なインメモリデフォルト実装の需要が高まっていた。

## 2. Problem Statement (問題定義)

1. **技術型フォルダ構成の陳腐化**: `Abstractions/`, `Strategies/`, `Options/` といった水平分割フォルダにコードが分散し、機能単位（Retry, Timeout, CircuitBreaker 等）での凝集度が低かった。
2. **外部エンジンへの強制依存**: Polly が登録されていない環境ではフォールバックが機能せず、ローカル開発やシンプルな単体テストでも DI コンテナのセットアップが煩雑であった。
3. **エラーモデルの非統一**: タイムアウトやリトライ超過時の失敗が例外（`ResilienceException`）で送出され、VK.Blocks の統一 Result パターン（`CS.01`, `BASE-05`）に違反していた。

## 3. Decision (決定事項)

`VK.Blocks.Resilience` を **垂直切片（Feature Slice）構造** へ刷新し、切片ごとに独立したプロトコル、オプション、および自作のローカル・インメモリ実装を提供する。

1. **垂直切片（Vertical Slice）ディレクトリ構造（`BB.01`）**:
   - `Bulkhead/`, `CircuitBreaker/`, `Fallback/`, `RateLimiting/`, `Retry/`, `Timeout/` の各ドメインフォルダに集約。
   - 各切片に `[VKFeature(typeof(VKResilienceBlock))]` を付与した Feature クラス（例: `RetryFeature.cs`）を配置。
2. **切片個別インターフェースと Local 実装の同梱（`ARCH-05`）**:
   - `IVKRetryExecutor` ➔ `LocalRetryExecutor`
   - `IVKCircuitBreaker` ➔ `LocalCircuitBreaker`
   - `IVKTimeoutExecutor` ➔ `LocalTimeoutExecutor`
   - `IVKRateLimiter` ➔ `LocalRateLimiter`
   - `IVKBulkhead` ➔ `LocalBulkhead`
   - `IVKFallbackHandler` ➔ `LocalFallbackHandler`
3. **Result パターン準拠のエラー定数マトリクス（`CS.01`）**:
   - `Common/Errors/VKResilienceErrors.cs` を新設し、`Timeout`, `RetryExhausted`, `CircuitBreakerOpen`, `RateLimitExceeded`, `BulkheadExceeded` などの不変エラー定数を一元管理。

## 4. Alternatives Considered (代替案の検討)

- **Option 1: ADR-001 の大域パイプライン設計を維持**:
  - *Approach*: 1つの大きな `IResiliencePipeline` のみを公開し続け、内部で各戦略を合成。
  - *Rejected Reason*: リトライ単体やタイムアウト単体をピンポイントで差し込みたい呼び出し側の柔軟性を奪うため却下。
- **Option 2: 各機能を独立した NuGet パッケージに完全分離**:
  - *Approach*: `VK.Blocks.Resilience.Retry`, `VK.Blocks.Resilience.Timeout` 等に細分化。
  - *Rejected Reason*: パッケージ管理オーバーヘッドが急増し、耐障害性という単一ドメイン内での過剰分割（`ARCH-04` Packaging Integrity 違反）となるため却下。

## 5. Consequences & Mitigation (結果と緩和策)

- **Positive**:
  - 各機能が自己完結した切片となり、高凝集な設計を達成。
  - 外部エンジンなしで完全な In-Memory 動作が可能となり、単体テストやモックの容易性が飛躍的に向上。
  - 例外送出から `VKResult<T>` による型安全なエラーハンドリングへ統一。
- **Negative**:
  - 手作りの `LocalBulkhead`（セマフォ）や `LocalCircuitBreaker`（状態マシン）をライブラリ内に抱え込むことになり、長期的な保守負担が増加（のちに ADR-005 にて Polly v8 委譲へ再移行）。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- すべてのクラスを `sealed` 化し、Options は `sealed partial record : IVKBlockOptions` に統一。
- `LocalCircuitBreaker` はスレッドセーフな状態管理（`Interlocked` やロック）を内包。
- `VKResilienceErrors` による RFC 7807 準拠のエラーコード体系（`Resilience.{Feature}`）を確立。

