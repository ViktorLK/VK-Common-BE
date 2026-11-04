# RFC-001: V1 Foundation & Zero-Dependency Authentication Core

- **Status**: ✅ Approved (Implemented Evolution Wave 1)
- **Author**: Principal Architect & AI Pair
- **Date**: 2026-10-29
- **Target Module(s)**: `VK.Blocks.Authentication`, `VK.Blocks.Authentication.StackExchangeRedis`
- **Related ADRs**:
  - [ADR-001: Remove Dead Code and Unused Abstractions](/docs/04-ArchitectureDecisionRecords/Authentication/adr-001-remove-dead-code-and-unused-abstractions.md)
  - [ADR-002: RFC 7807 Compliant Error Responses](/docs/04-ArchitectureDecisionRecords/Authentication/adr-002-rfc-7807-compliant-error-responses.md)
  - [ADR-003: Consolidate Token Blacklist Check Responsibility](/docs/04-ArchitectureDecisionRecords/Authentication/adr-003-consolidate-token-blacklist-check-responsibility.md)
  - [ADR-004: Comprehensive Refactoring of Authentication Module (Phase 1-3.5)](/docs/04-ArchitectureDecisionRecords/Authentication/adr-004-comprehensive-refactoring-of-authentication-module-phase-1-35.md)
  - [ADR-005: Introduce Atomic Rate Limiting for API Keys](/docs/04-ArchitectureDecisionRecords/Authentication/adr-005-introduce-atomic-rate-limiting-for-api-keys.md)
  - [ADR-006: Refactor Authentication Core for Zero-Dependency and InMemory Defaults](/docs/04-ArchitectureDecisionRecords/Authentication/adr-006-refactor-authentication-core-for-zero-dependency.md)
  - [ADR-007: Optimize High-Frequency Auth Interfaces with ValueTask](/docs/04-ArchitectureDecisionRecords/Authentication/adr-007-optimize-high-frequency-auth-interfaces-with-valuetask.md)
  - [ADR-012: Authentication Module Standardization and Idempotent DI Registration](/docs/04-ArchitectureDecisionRecords/Authentication/adr-012-authentication-module-standardization.md)
  - [ADR-013: Autonomous Lifecycle Management for Cleanup Tasks](/docs/04-ArchitectureDecisionRecords/Authentication/adr-013-autonomous-cleanup-lifecycle-management.md)
  - [ADR-017: Standardize Lifecycle Management in In-Memory Authentication Providers](/docs/04-ArchitectureDecisionRecords/Authentication/adr-017-standardize-lifecycle-management-in-in-memory-authentication-providers.md)

---

## 1. 🎯 Motivation & Problem Statement (背景と課題定義)

- **第1波着手前の課題 (Wave 1 Pain Points)**:
  - **インフラ過度結合 (`ADR-006`)**: 認証コアライブラリが `IDistributedCache` や Redis クライアントに直接依存しており、ローカル開発・単体テスト時にも Redis インフラの稼働が強制されていた。
  - **高頻度リクエスト時の GC 圧迫 (`ADR-007`)**: API Key 検証やレート制限などの全 HTTP リクエストが通過するホットパスで `Task<T>` ヒープアロケーションが多発し、スループット低下と GC ポーズを招いていた。
  - **検証パイプラインの二重 I/O (`ADR-003`)**: JWT トークンの失効（Blacklist）検証がミドルウェアとハンドラの両方で重複実行され、不要な I/O オーバーヘッドが発生していた。
  - **DI 登録の競合と汚染 (`ADR-012`)**: 多重登録時の挙動が非決定的であり、`TryAdd` 規則の欠落により下流サービスによるカスタマイズが困難であった。
  - **不要なリソース消費 (`ADR-013`, `ADR-017`)**: 分散キャッシュ（Redis）に切り替えた後も、InMemory 用の定期クリーンアップタイマーがバックグラウンドで無駄に起動し続けていた。

- **なぜ今やるのか (Why Now / Consumer — `BASE-06`)**:
  - `VK.Blocks` を利用するすべての API ホスト（Web API、BFF、マイクロサービス）が、外部インフラの有無にかかわらず即座にモック不要で起動・開発・テストできる「ゼロ依存・自己完結型」の認証基盤を確立するため。

---

## 2. 🧭 Goals & Non-Goals (目標と非目標)

- **Goals (達成すべき目標)**:
  - **Zero-Dependency Core (`ARCH-04`, `ARCH-05`)**: コアパッケージから外部分散キャッシュ依存を完全剥離し、単一プロセスで完結するスレッドセーフな `InMemory` プロバイダーをデフォルト提供する。
  - **Zero-Allocation Hot Path (`CS.04`)**: 高頻度認証インターフェイスの戻り値を `ValueTask` に移行し、同期完了キャッシュヒット時のヒープ割り当てをゼロにする。
  - **自律的ライフサイクル管理 (`ADR-013`)**: 分散プロバイダーの登録有無を自律検知し、不要な InMemory クリーンアップワーカーを自己停止（Hard Exit）させる。
  - **単一責任のトークン失効照合 (`ADR-003`)**: 失効チェックを `JwtBearerEventsFactory.OnTokenValidated` の 1 箇所へ集約。
  - **べき等な DI 構成 (`AP.02`)**: 全サービス登録を `TryAdd` / `TryAddEnumerable` で標準化し、順序非依存・多重登録耐性を保証する。

- **Non-Goals (スコープ外 — `BASE-06` 準拠)**:
  - **認可ポリシーの評価**: 権限、ロール、リソース単位の認可判定は対象外（`VK.Blocks.Authorization` の責務）。
  - **分散セッションのコア内包**: Redis や Azure Table Storage などの分散ストレージ実装はコアに含めず、専用の拡張パッケージ（`Authentication.StackExchangeRedis`）へ分離する。
  - **Source Generator による全自動化**: この波では手動による不変性担保とリファクタリングを優先し、コードジェネレータ化は第 3 波へ委ねる。

---

## 3. 🧬 Metaphor & Mapping (メンタルモデルと構造マッピング — `PS.03`)

> **Metaphor (概念メタファー)**: **国境検問所の自立型インフラ整備 (The Self-Sustaining Border Checkpoint)**
>
> 従来の検問所は、外部の巨大な中央データセンター（Redis）と専用回線が常時接続されていないとゲートすら開けられませんでした。
> 第 1 波の改革により、各検問所に自家発電機とローカル帳簿（InMemory プロバイダー）を配備し、外部回線がなくても自給自足で即座に入国審査を開始できるようにしました。
> また、通行証（API Key）の確認には高速優先レーン（`ValueTask`）を敷設し、渋滞（GC ポーズ）を解消。さらに、中央データセンターとの本線が開通した際には、地元の不要な巡回清掃員（InMemory Cleanup Worker）が自動的に作業を停止して中央管理へ委ねる自律協調機構を備えました。

| Metaphor Component (概念要素) | Technical Artifact (技術要素) | Layer / Scope | Responsibility (責務) |
| :--- | :--- | :--- | :--- |
| **Local Ledger (自給式帳簿)** | `InMemoryTokenRevocationProvider`, `InMemoryApiKeyRateLimiter` | `Internal/` | 外部インフラなしで完結するプロセス内スレッドセーフな失効・レート制限 |
| **Express Check Lane (高速優先レーン)** | `IVKApiKeyRateLimiter`, `IVKJwtAuthService` | Public Protocol | `ValueTask` によるホットパスのゼロアロケーション検証 |
| **Single Gatekeeper (単一検問官)** | `JwtBearerEventsFactory.OnTokenValidated` | `Internal/` | 二重検証を排除した単一地点でのトークン有効性・失効判定 |
| **Autonomous Janitor (自律巡回清掃員)** | `InMemoryCleanupBackgroundService` | `Internal/` | 分散ストア未接続時のみ動作し、接続時は自律停止するメモリ回収タスク |
| **Central Power Grid (中央送電網)** | `Authentication.StackExchangeRedis` | Dedicated Block | 大規模分散環境時のみオプトイン導入される外部インフラ連携アダプター |

---

## 4. 🏗️ Contract & Code Blueprint (契約と主要インターフェース設計 — `BASE-01`)

### 4.1 Public Protocols & Zero-Allocation Models (`Common/Protocols/`)

高頻度ホットパスにおけるアロケーションを抑えるため、プロバイダーインターフェイスに `ValueTask` を採用します（`ADR-007`）。

```csharp
namespace VK.Blocks.Authentication;

// Public Protocol: 高頻度 API Key レート制限契約 (ADR-005, ADR-007)
public interface IVKApiKeyRateLimiter
{
    /// <summary>
    /// Evaluates if the API key request is allowed under current quota.
    /// Returns ValueTask for zero heap-allocation on synchronous cache hits.
    /// </summary>
    ValueTask<VKResult<bool>> CheckLimitAsync(
        string apiKey, 
        int maxRequests, 
        TimeSpan window, 
        CancellationToken cancellationToken = default);
}

// Public Protocol: トークン失効判定契約 (ADR-003, ADR-006)
public interface IVKTokenRevocationProvider
{
    ValueTask<VKResult<bool>> IsRevokedAsync(
        string identifier, 
        CancellationToken cancellationToken = default);

    Task<VKResult> RevokeAsync(
        string identifier, 
        TimeSpan ttl, 
        CancellationToken cancellationToken = default);
}
```

### 4.2 Error Contract (`VKAuthenticationErrors`, `VKJwtErrors`)

RFC 7807 準拠の Problem Details と整合する型付きエラーを定義（`ADR-002`, `CS.01`）。

```csharp
namespace VK.Blocks.Authentication;

public static class VKAuthenticationErrors
{
    public static readonly VKError RateLimitExceeded = new(
        "Auth.RateLimitExceeded", 
        "Rate limit exceeded. Please retry after some time.", 
        VKErrorType.Validation);

    public static readonly VKError TokenRevoked = new(
        "Auth.TokenRevoked", 
        "The authentication token has been revoked.", 
        VKErrorType.Unauthorized);
}
```

---

## 5. ⚖️ Trade-offs & Risk Analysis (トレードオフと非機能リスク)

- **Performance & Concurrency (`CS.04`)**:
  - `InMemoryTokenRevocationProvider` および `InMemoryApiKeyRateLimiter` は `ConcurrentDictionary` と `ReaderWriterLockSlim` を用いてロック競合を最小化。
  - レート制限のインクリメント処理において、マルチノード時は Redis の Lua スクリプト（`ADR-005`）により Race Condition を完全排除。
- **Memory Footprint & Leak Prevention (`ADR-013`, `ADR-017`)**:
  - `InMemory` モードでは、期限切れエントリが蓄積してメモリリークを引き起こさないよう、バックグラウンドタイマーが定期クリーンアップを自律遂行。
  - 分散プロバイダーが登録されている場合は、起動直後に `IHostApplicationLifetime` を検知してタイマー起動を抑制（Hard Exit）。
- **Alternatives Considered**:
  - *代替案 A: `Microsoft.Extensions.Caching.Memory.IMemoryCache` への全面依存*
    - **却下理由 (`ADR-006`)**: 単純なキー失効に対して `IMemoryCache` はオーバーヘッドが大きく、細かい有効期限制御とアトミックな増減操作が困難であるため。
  - *代替案 B: すべての戻り値を `Task<T>` のまま維持*
    - **却下理由 (`ADR-007`)**: リクエスト毎の検証処理において、キャッシュヒット時にも Task オブジェクトがヒープ確保され、マイクロベンチマークで有意な GC 負荷が観測されたため。

---

## 6. 📋 Decomposition & Implementation Plan (段階的実装とバックログ分解計画)

本波で完了した実装タスク群（トレーサビリティ担保）：

- [x] **Step 1 (Dead Code & Error Standardization)**:
  - 未使用の `AuthResult` 削除と RFC 7807 準拠エラー定数の整備（`ADR-001`, `ADR-002`）。
- [x] **Step 2 (Zero-Dependency & In-Memory Fallbacks)**:
  - Redis 依存をコアから `Authentication.StackExchangeRedis` へ切り離し、`InMemory` プロバイダーをコアへ同梱（`ADR-006`）。
- [x] **Step 3 (Hot-Path Optimization)**:
  - `IVKApiKeyRateLimiter` および `IVKTokenRevocationProvider` のシグネチャを `ValueTask` に移行（`ADR-007`）。
- [x] **Step 4 (Single Pipeline Consolidation)**:
  - `JwtBearerEventsFactory.OnTokenValidated` への失効チェック一元化（`ADR-003`）。
- [x] **Step 5 (DI & Autonomous Lifecycle)**:
  - `TryAdd` べき等登録パイプラインと `InMemoryCleanupBackgroundService` の自律停止制御（`ADR-012`, `ADR-013`, `ADR-017`）。

