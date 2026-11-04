# RFC-001: V1 Foundation, Result Pattern & Deterministic Core Contracts

- **Status**: ✅ Approved (Implemented Evolution Wave 1)
- **Author**: Principal Architect & AI Pair
- **Date**: 2026-10-29
- **Target Module(s)**: `VK.Blocks.Core`
- **Related ADRs**:
  - [ADR-001: Expanding Global ErrorType for Enterprise-Grade Error Handling](/docs/04-ArchitectureDecisionRecords/Core/adr-001-expanding-global-errortype-for-enterprise-grade-error-handling.md)
  - [ADR-002: Idempotent BuildingBlock Options Registration in DI Container](/docs/04-ArchitectureDecisionRecords/Core/adr-002-idempotent-buildingblock-options-registration-in-di-container.md)
  - [ADR-004: Unified Synchronization State Abstraction in Core](/docs/04-ArchitectureDecisionRecords/Core/adr-004-unified-synchronization-state-abstraction-in-core.md)
  - [ADR-010: High-Performance Infrastructure Optimizations in Core Library](/docs/04-ArchitectureDecisionRecords/Core/adr-010-high-performance-infrastructure-optimizations-in-core-library.md)
  - [ADR-011: Standardized Exception Hierarchy and Fault Modeling](/docs/04-ArchitectureDecisionRecords/Core/adr-011-standardized-exception-hierarchy-and-fault-modeling.md)
  - [ADR-013: Comprehensive Result Pattern Standardization with VKError](/docs/04-ArchitectureDecisionRecords/Core/adr-013-comprehensive-result-pattern-standardization-with-vkerror.md)
  - [ADR-015: Adopt Vertical Slices and Mandatory Core Abstractions in Building Blocks](/docs/04-ArchitectureDecisionRecords/Core/adr-015-adopt-vertical-slices-and-mandatory-core-abstractions-in-building-blocks.md)

---

## 1. 🎯 Motivation & Problem Statement (背景と課題定義)

- **第1波着手前の課題 (Wave 1 Pain Points)**:
  - **曖昧な HTTP エラー分類と障害可観測性の欠如 (`ADR-001`)**: 初期コアの `ErrorType` は単純な CRUD 向け（Validation, NotFound, Conflict）に限定されており、レートリミット超過（429）、外部サービス一時停止（503）、タイムアウト（504）などの分散システム障害を表現できず、すべて 500 や 400 に丸められていた。
  - **DI コンテナでのオプション重複登録とメモリ浪費 (`ADR-002`)**: 各モジュールのスタートアップ時に `AddVKBlockOptions` が複数回呼び出された際、検証ロジックや DI 記述子が多重登録され、起動遅延とメモリ肥大化を招いていた。
  - **同期指紋・ハッシュ管理の分散とボイラープレート (`ADR-004`)**: 権限やスキーマのメタデータ差分同期において、各モジュールが個別にストレージ依存コードを持ち込み、デフォルト実装の標準化がなされていなかった。
  - **ホットパスにおける GC 圧力と CA1000 警告 (`ADR-010`)**: 成功結果（`Result.Success()`）の生成時に都度ヒープ割り当てが発生し、またジェネリック型に静的メソッドを公開することでコード解析ツールから CA1000 警告が多発していた。
  - **例外の制御フロー悪用と RFC 7807 障害モデルの不統一 (`ADR-011`)**: 業務ロジックの失敗に `throw new Exception` が濫用され、パフォーマンスの悪化と呼び出し側でのエラーハンドリング破綻を招いていた。
  - **プリミティブな Result/Error 型による名前衝突とマジックストリング (`ADR-013`)**: 他のサードパーティライブラリとの型名競合が発生し、エラーコードの文字列直書きによるタイポや再利用性の欠落が横行していた。
  - **非決定論的 API による単体テストの破壊 (`ADR-015`)**: `DateTime.UtcNow` や `Guid.NewGuid()` の直接呼び出しがドメインコードに散在し、時間や ID に依存するロジックの決定論的テストが不可能となっていた。

- **なぜ今やるのか (Why Now / Consumer — `BASE-06`)**:
  - `VK.Blocks` を採用するすべての BuildingBlock（Persistence, Web, Auth, Storage, AI）および上位アプリケーションに対し、不確実性を排した「決定論的・安全・高スループットな基礎度量衡（Standard Weights & Measures）」を提供するため。

---

## 2. 🧭 Goals & Non-Goals (目標と非目標)

- **Goals (達成すべき目標)**:
  - **Result パターンと VKError の全面統一 (`ADR-013`, `CS.01`)**: `VKResult` / `VKResult<T>` をフレームワーク唯一の処理結果契約とし、静的 readonly な `VKError` 定数（`VKCoreErrors`）による型安全なエラー管理を義務化する。
  - **エンタープライズ級 ErrorType と RFC 7807 障害モデル (`ADR-001`, `ADR-011`, `FS-02`)**: `TooManyRequests` (429)、`ServiceUnavailable` (503)、`Timeout` (504)、`ExternalError` (502) を追加し、例外は回復不能なインフラ障害のみに限定して `VKBaseException` 経由で RFC 7807 ProblemDetails へ一貫マッピングする。
  - **ゼロアロケーション最適化と InnerCache パターン (`ADR-010`)**: `VKResult.Success()` のシングルトンキャッシュ、ジェネリック型内部キャッシュ（`InnerCache`）、および `AggressiveInlining` を適用してホットパスの GC 負荷を極小化する。
  - **決定論的コア抽象の強制 (`ADR-015`, `CS.06`)**: `IVKGuidGenerator`（順序付き GUID 生成）および .NET 標準の `TimeProvider` のみを許容し、直接のシステムクロック・乱数取得を静的解析レベルで禁止する。
  - **統一同期状態抽象と No-Op フォールバック (`ADR-004`, `ARCH-05`)**: `ISyncStateStore` 抽象を定義し、デフォルトで安全に動作する `VKNoOpSyncStateStore` を Core 同一パッケージに同梱する。
  - **DI オプション登録の冪等性基礎 (`ADR-002`)**: `services.Any()` チェックと `TryAddSingleton` の組み合わせにより、多重登録を安全に防ぐ。

- **Non-Goals (スコープ外 — `BASE-06` 準拠)**:
  - **ドメインエンティティやリポジトリの具体的永続化**: EF Core や Dapper などのインフラ実装は `VK.Blocks.Persistence` の責務であり、Core は純粋な抽象（`IVKEntity`, `IVKRepository`）のみを定義する。
  - **リクエスト動的パラメータのコード生成**: リクエストスコープでの上書き生成エンジンは第 3 波（Wave 3）へ委託する。
  - **Source Generator による依存関係自動検証**: コンパイル時トポロジー検証は第 2 波（Wave 2）へ委託する。

---

## 3. 🧬 Metaphor & Mapping (メンタルモデルと構造マッピング — `PS.03`)

> **Metaphor (概念メタファー)**: **重工業の基本度量衡と堅牢な鋳造所 (The Standard Weights & Measures and Foundational Foundry)**
>
> 近代的な巨大工場群（マイクロサービスや各 BuildingBlock）が円滑に稼働するためには、ミリメートル単位の厳格な「国家度量衡（長さ・重さ・時間の共通基準）」と、一切の不純物を含まない「標準インゴット（共通データ通貨）」が不可欠です。
> 職人がそれぞれ勝手な定規（`DateTime.UtcNow` や生 `Guid`）で部品を削ったり、不良品（障害）を「爆発（例外スロー）」で伝達していては、コンベア全体が停止してしまいます。
> 第 1 波の改革により、Core は共通の測定原器（`TimeProvider`, `IVKGuidGenerator`）を制定し、すべての部品の合否を精密な規格印（`VKResult`, `VKError`）で判定する鋳造所を建設しました。また、良品判定（`Success`）には一度鋳造した完璧なインゴットを再利用する保管庫（`InnerCache`）を備え、無駄な鉄屑（GC アロケーション）の発生を原理的に根絶しました。

| Metaphor Component (概念要素) | Technical Artifact (技術要素) | Layer / Scope | Responsibility (責務) |
| :--- | :--- | :--- | :--- |
| **標準インゴット (Universal Currency)** | `VKResult`, `VKResult<T>` | `Results/` (Public) | 成功・失敗を明示し、例外を用いずに下流へ伝搬する唯一の通貨（ADR-013, CS.01） |
| **欠陥精密刻印 (Defect Stamp)** | `VKError`, `VKErrorType` | `Errors/` (Public) | 障害原因の識別子、メッセージ、HTTP/gRPC 分類を保持する不変値（ADR-001, ADR-013） |
| **金型保管庫 (Immutable Ingot Cache)** | `VKResult.Success()`, `InnerCache<T>` | `Results/` (Internal) | 頻出する成功インスタンスを事前キャッシュしヒープ割り当てをゼロ化（ADR-010） |
| **国家標準時計・造幣刻印機 (Standard Instruments)** | `TimeProvider`, `IVKGuidGenerator` | `Guids/`, BCL | 決定論的な現在時刻と順序付き GUID を供給する抽象（ADR-015, CS.06） |
| **事故公式調査書 (Incident Dossier)** | `VKBaseException` | `Exceptions/` (Public) | 回復不能なインフラ障害のコンテキストを RFC 7807 形式で保持（ADR-011） |
| **工場間同期指紋 (Synchronized Registry)** | `ISyncStateStore`, `VKNoOpSyncStateStore` | `Synchronization/` | 各ブロックのメタデータ同期ハッシュを保持し冪等性を保証（ADR-004） |

---

## 4. 🏗️ Contract & Code Blueprint (契約と主要インターフェース設計 — `BASE-01`)

### 4.1 Result & Error Contract (`Results/`, `Errors/`)

```csharp
namespace VK.Blocks.Core.Results;

using VK.Blocks.Core.Errors;

// Public Result Contract (CS.01 Result<T>, AP.01 Sealed)
public sealed class VKResult
{
    private static readonly VKResult CachedSuccess = new(true, VKError.None);

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public VKError Error { get; }

    internal VKResult(bool isSuccess, VKError error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static VKResult Success() => CachedSuccess;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static VKResult Failure(VKError error) => new(false, error);
}

public sealed class VKResult<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T Value { get; }
    public VKError Error { get; }

    internal VKResult(bool isSuccess, T value, VKError error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static VKResult<T> Success(T value) => new(true, value, VKError.None);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static VKResult<T> Failure(VKError error) => new(false, default!, error);
}
```

### 4.2 Standard Error Codes (`VKCoreErrors`)

```csharp
namespace VK.Blocks.Core.Errors;

public static class VKCoreErrors
{
    public static readonly VKError GeneralFailure = new(
        "Core.GeneralFailure",
        "An unexpected error occurred during execution.",
        VKErrorType.Failure);

    public static readonly VKError ValidationFailed = new(
        "Core.ValidationFailed",
        "One or more validation rules were violated.",
        VKErrorType.Validation);

    public static readonly VKError DependencyNotFound = new(
        "Core.DependencyNotFound",
        "A required building block or service dependency was not found in the service provider.",
        VKErrorType.NotFound);

    public static readonly VKError OperationTimeout = new(
        "Core.Timeout",
        "The requested operation exceeded the configured timeout limit.",
        VKErrorType.Timeout);

    public static readonly VKError ServiceUnavailable = new(
        "Core.ServiceUnavailable",
        "The underlying service or resource is temporarily unavailable.",
        VKErrorType.ServiceUnavailable);
}
```

### 4.3 Deterministic Guids & Synchronization Abstractions (`Guids/`, `Synchronization/`)

```csharp
namespace VK.Blocks.Core.Guids;

public interface IVKGuidGenerator
{
    Guid NewGuid();
    Guid CreateSequentialGuid();
}

namespace VK.Blocks.Core.Synchronization;

public interface ISyncStateStore
{
    Task<string?> GetFingerprintAsync(string key, CancellationToken cancellationToken = default);
    Task SetFingerprintAsync(string key, string fingerprint, CancellationToken cancellationToken = default);
}

// Fallback Tier No-Op Implementation (ARCH-05)
internal sealed class VKNoOpSyncStateStore : ISyncStateStore
{
    public Task<string?> GetFingerprintAsync(string key, CancellationToken cancellationToken = default) =>
        Task.FromResult<string?>(null);

    public Task SetFingerprintAsync(string key, string fingerprint, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
```

---

## 5. ⚖️ Trade-offs & Risk Analysis (トレードオフと非機能リスク)

- **Performance & Zero-Allocation Strategy (`ADR-010`)**:
  - `VKResult.Success()` の単一インスタンスキャッシュにより、毎秒数万件の成功パスにおいてヒープ割り当てを実質ゼロに圧縮。
  - C# ジェネリックの静的メンバ警告（CA1000）を回避するため、内部クラス `InnerCache<T>` パターンを採用し、型安全性と静的解析コンプライアンスを両立。
- **Deterministic Logic & Pure Abstractions (`ADR-015`, `CS.06`)**:
  - `TimeProvider` と `IVKGuidGenerator` を DI 経由で注入することで、単体テストにおける時間経過シミュレーション（仮想時計）や順序検証を確実に行える。
- **Alternatives Considered**:
  - *代替案 A: 例外（Exception）によるドメイン失敗の通知 (`ADR-011`)*
    - **却下理由**: スタックトレースのキャプチャによる著しい CPU コスト（1 回の throw で数マイクロ秒）が発生し、フロー制御としての例外利用は可読性と保守性を阻害するため。
  - *代替案 B: エラーコードの自由記述（Magic String）(`ADR-013`)*
    - **却下理由**: タイポの温床となり、国際化（i18n）や監視ツールでの集計キーとして破綻するため。`VKError` による定数定義を必須化。

---

## 6. 📋 Decomposition & Implementation Plan (段階的実装とバックログ分解計画)

本波で完了した実装タスク群（トレーサビリティ担保）：

- [x] **Step 1 (Error System Expansion)**:
  - `VKErrorType` への 429, 503, 504 等の追加と HTTP マッピングの拡張（`ADR-001`）。
- [x] **Step 2 (DI Options Idempotency)**:
  - `AddVKBlockOptions` における重複登録防止と `Any()` ガードの導入（`ADR-002`）。
- [x] **Step 3 (Synchronization Abstraction)**:
  - `ISyncStateStore` および `VKNoOpSyncStateStore` の導入（`ADR-004`）。
- [x] **Step 4 (Zero-Allocation & Cache Optimization)**:
  - `InnerCache` パターンと `AggressiveInlining` による CA1000 解消と GC 負荷低減（`ADR-010`）。
- [x] **Step 5 (Exception Hierarchy)**:
  - `VKBaseException` と RFC 7807 準拠の構造化例外基盤の確立（`ADR-011`）。
- [x] **Step 6 (Result Pattern Overhaul)**:
  - `Result` から `VKResult` / `VKError` へのリネームと `VKCoreErrors` の制定（`ADR-013`）。
- [x] **Step 7 (Deterministic Contracts & Slices)**:
  - `IVKGuidGenerator`、`TimeProvider` の義務化と垂直切片フォルダ構成への移行（`ADR-015`）。
