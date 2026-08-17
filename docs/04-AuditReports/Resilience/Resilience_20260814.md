# 🏗️ アーキテクチャ監査レポート: VK.Blocks.Resilience

> **日付**: 2026-08-14  
> **対象**: [Resilience](/src/BuildingBlocks/Resilience/)  
> **Auditor**: AI Architecture Audit (Phase 1-4)  
> **Handshake**: `Active: [L1+L2:Resilience] | Context: src/BuildingBlocks/Resilience | Sync: [AP.03,BB.01,BB.02,BB.03,BB.04,BB.05,AP.02,CS.02,CS.01,AP.01,CS.03,CS.06,OR.01,BB.07,DL.01]`  
> **Audit**: ⚠️ 軽微な指摘あり

---

## 📊 監査サマリー (Audit Summary)

- **総合スコア**: 88/100
- **Fast Audit スコア**: 28/30 (93%)
- **対象レイヤー判定**: BuildingBlock / Infrastructure Abstraction Layer
- **総評 (Executive Summary)**:

VK.Blocks.Resilience は、6つの独立したレジリエンス戦略（Retry, Timeout, CircuitBreaker, Fallback, RateLimiting, Bulkhead）を垂直スライス構造で提供する、アーキテクチャ的に成熟したモジュールである。SG自動化によるDI登録、`sealed record` + `IVKBlockOptions` のOptions設計、`VKResult<T>` ベースのエラーハンドリング、`VKGuard` による境界防御、`TimeProvider` 注入による非確定的API排除など、VK.Blocks Industrial DNA への準拠度は高い。主要な改善点は、①エラー定数のインライン化（CS.01）、②`LocalCircuitBreaker` の `VKGuard` 未使用・`IVKCircuitBreaker` と `VKCircuitBreakerOptions` の設計不整合、③`ResilienceException` の `innerException` 未伝播、④`IVKResiliencePipeline` の未実装状態、にある。

---

## ⚡ Fast Audit スコア詳細

### 📁 Structure (BB.01)

| ID | Check | Result |
|:---|:------|:-------|
| S-01 | Feature Slices present | ✅ `Bulkhead/`, `CircuitBreaker/`, `Fallback/`, `RateLimiting/`, `Retry/`, `Timeout/` |
| S-02 | SG-generated DI (`[VKBlockMarker]`) | ✅ SG pipeline active |
| S-03 | `Diagnostics/` or SG Diagnostics | ✅ [Common/Diagnostics/](/src/BuildingBlocks/Resilience/Common/Diagnostics/) |
| S-04 | `Diagnostics/Internal/` | ✅ [Common/Diagnostics/Internal/](/src/BuildingBlocks/Resilience/Common/Diagnostics/Internal/) |
| S-05 | Marker file at root | ✅ [VKResilienceBlock.cs](/src/BuildingBlocks/Resilience/VKResilienceBlock.cs) |
| S-06 | Options co-located in Feature Slices | ✅ 各Feature内に配置 |

### 🏷️ Marker (BB.02)

| ID | Check | Result |
|:---|:------|:-------|
| M-01 | `[VKBlockMarker]` attribute | ✅ Found |
| M-02 | Legacy `IVKBlockMarker` | ✅ Not found (no legacy) |
| M-03 | `sealed partial class` | ✅ `public sealed partial class VKResilienceBlock;` |
| M-04 | Dependencies declared | ✅ `Dependencies = [typeof(VKCoreBlock)]` |

### 🔌 DI Registration (BB.03, AP.02/04)

| ID | Check | Result |
|:---|:------|:-------|
| D-01 | `IsVKBlockRegistered` | ✅ SG-generated (`ResilienceBlockRegistration.g.cs` L25) |
| D-02 | `AddVKBlockMarker` | ✅ SG-generated (`ResilienceBlockRegistration.g.cs` L34) |
| D-03 | `AddVKBlockOptions` | ✅ SG-generated (`ResilienceBlockRegistration.g.cs` L31) |
| D-04 | `TryAdd` pattern | ✅ 全Feature登録で使用 |
| D-05 | No direct `services.Add(Singleton|...)` | ✅ 検出なし |
| D-06 | Wrapper → Internal delegation | ✅ SG-generated |
| D-07 | `AddVK.*Block` naming | ✅ SG-generated `AddVKResilienceBlock` |

### ⚙️ Options (BB.05, AP.04)

| ID | Check | Result |
|:---|:------|:-------|
| O-01 | `sealed record` + `IVKBlockOptions` | ✅ 全6 Options |
| O-02 | `VK` prefix | ✅ `VKTimeoutOptions`, `VKRetryOptions`, etc. |
| O-03 | `SectionName` defined | ✅ SG-generated (`VKResilienceOptions.g.cs` L16) |
| O-04 | No legacy `sealed class` | ✅ 検出なし |

### 🔍 Implementation Patterns

| ID | Check | Result |
|:---|:------|:-------|
| I-01 | Sealed ratio | ✅ 8 `public sealed` / 0 `public class` (100%) |
| I-02 | VKGuard usage | ✅ 6 instances in boundaries |
| I-03 | ConfigureAwait | ✅ 8/8 `await` with `ConfigureAwait(false)` (100%) |
| I-04 | `[LoggerMessage]` SG | ⚠️ 未検出 — モジュールにロギング実装なし（メトリクスのみ） |
| I-05 | No direct logger calls | ✅ 検出なし |
| I-06 | `[VKBlockDiagnostics]` | ✅ [ResilienceDiagnostics.cs](/src/BuildingBlocks/Resilience/Common/Diagnostics/Internal/ResilienceDiagnostics.cs) |
| I-07 | Result pattern | ✅ `VKResult<T>` / `VKResult.Failure<T>` 多数使用 |
| I-08 | No `DateTime.UtcNow` | ✅ 検出なし |
| I-09 | No `Guid.NewGuid()` | ✅ 検出なし |
| I-10 | No raw `JsonSerializer` | ✅ 検出なし |
| I-11 | No EF/Redis deps | ✅ 検出なし |

### 📛 Naming & Visibility (AP.03)

| ID | Check | Result |
|:---|:------|:-------|
| N-01 | Public types use `VK`/`IVK` prefix | ✅ 検出なし（全て準拠） |
| N-02 | Internal dir types are `internal` | ✅ 検出なし（全て準拠） |
| N-03 | Namespace matches path | ✅ |

### 📊 Summary Table

| Category | Tier | ✅ | ❌ | ⚠️ |
|:---------|:-----|:---|:---|:---|
| Structure | 🟡 | 6 | 0 | 0 |
| Marker | 🔴 | 4 | 0 | 0 |
| DI Registration | 🔴 | 7 | 0 | 0 |
| Options | 🟡 | 4 | 0 | 0 |
| Implementation | 🔴 | 10 | 0 | 1 |
| Naming | 🟡 | 3 | 0 | 0 |

**Fast Audit Score**: 28/30 (93%) — I-04 のみ ⚠️（ロギング不在は運用段階で要対応）

---

## Phase 2: Registration Audit (DI Layer)

### BB.03 実行順序 ✅

SG-generated [ResilienceBlockRegistration.g.cs](/src/BuildingBlocks/Resilience/obj/Generated/VK.Tools.SourceGenerators/VK.Tools.SourceGenerators.DependencyInjection.VKBlockGenerator/ResilienceBlockRegistration.g.cs) の登録順序:

1. **Check-Self** (L25): `services.IsVKBlockRegistered<VKResilienceBlock>()`
2. **Options Registration** (L31): `services.AddVKBlockOptions<VKResilienceOptions>(configuration, transform)`
3. **Mark-Self** (L34): `services.AddVKBlockMarker<VKResilienceBlock>()`
4. **Validate Options** (L37): `TryAddEnumerableSingleton<IValidateOptions<VKResilienceOptions>, ...>()`
5. **Options Provider** (L40): `TryAddSingleton<IVKResilienceOptionsProvider, ...>()`
6. **Feature Toggle** (L45): `if (!options.Enabled) return builder;`
7. **Custom Hook** (L51): `VKResilienceBlock.Register(builder);`

→ **BB.03準拠**: 全8ステップが正しい順序で実装 ✅

### ADR-016 Func 変換 ✅

`ResilienceBlockRegistration.Register` のシグネチャ (L22):
```csharp
Func<VKResilienceOptions, VKResilienceOptions>? transform = null
```
→ `Func<T, T>` パターン準拠 ✅

### Enabled Policy Position ✅

`if (!options.Enabled)` (L45) は `AddVKBlockMarker` (L34) の **後** に配置 ✅

### Builder Pattern ✅

`ResilienceBlockBuilder` は `IVKResilienceBuilder` を返し、`TryAdd` 拡張を使用 ✅

### OptionsValidator Quality ✅

SG-generated [ResilienceBlock.g.cs](/src/BuildingBlocks/Resilience/obj/Generated/VK.Tools.SourceGenerators/VK.Tools.SourceGenerators.DependencyInjection.VKBlockGenerator/ResilienceBlock.g.cs) の `OptionsValidator` (L39-52):
- `IValidateOptions<VKResilienceOptions>` 実装
- `ValidateBlockCustom` partialフック経由でカスタムバリデーション可能
- 各Feature も `ValidateFeatureCustom` で個別プロパティバリデーション実装済

**Phase 2 結果**: PASS ✅

---

## Phase 3: Implementation Audit (Deep Analysis)

### 設計原則 (Design Principles)

**SOLID準拠度: ⭐⭐⭐⭐☆**

| 原則 | 評価 | 詳細 |
|:-----|:-----|:-----|
| **SRP** | ✅ | 各Feature は単一のレジリエンス戦略に対して責務が限定されている |
| **OCP** | ✅ | `IVK*` インターフェース経由で実装の差し替えが可能 |
| **LSP** | ✅ | 全てのLocal実装がインターフェース契約に準拠 |
| **ISP** | ✅ | 各インターフェースは最小限のメソッドのみ定義 |
| **DIP** | ✅ | `TimeProvider` 注入による非確定的API排除 |

**KISS / YAGNI / DRY: ⭐⭐⭐⭐⭐**
- 各実装はシンプルかつ必要最小限。ライブラリ依存なし（Polly等の外部依存排除）。

### 設計パターン (Design Patterns)

| パターン | 使用箇所 | 評価 |
|:---------|:---------|:-----|
| **Strategy** | `IVKTimeoutExecutor` / `LocalTimeoutExecutor` 等 | ✅ 適切 |
| **Feature Flag (Toggle)** | `VKResilienceOptions.Enabled` + SG Feature Toggle | ✅ 適切 |
| **Builder** | `IVKResilienceBuilder` + 機能チェーン | ✅ 適切 |
| **Pipeline** | `IVKResiliencePipeline` (インターフェース定義のみ) | ⚠️ 実装未完了 |

### 架構原則 (Architectural Principles)

- **関心の分離**: ✅ Feature Slice 毎に Options / Interface / Implementation / Registration を完全分離
- **封装性**: ✅ `Internal/` 配下に全実装を隠蔽。public API は `Protocols/` のインターフェースのみ
- **凝集度**: ✅ 各Feature は高凝集
- **結合度**: ✅ Core への依存のみ。Feature間の結合なし

### アーキテクチャ風格 (Architectural Styles)

- **Clean Architecture**: ✅ Domain ← App ← Infra の依存方向が正しい
- **Vertical Slice**: ✅ 6つの垂直スライス（Timeout, Retry, CircuitBreaker, Fallback, RateLimiting, Bulkhead）

### VK.Blocks 固有の準拠度 (Deep)

#### エラー定数パターン (CS.01)

⚠️ **指摘**: エラーコードがインラインのリテラル文字列として使用されている。

| ファイル | 行 | コード |
|:---------|:---|:-------|
| [LocalTimeoutExecutor.cs](/src/BuildingBlocks/Resilience/Timeout/Internal/LocalTimeoutExecutor.cs#L36) | L36 | `"Resilience.Timeout"` (inline) |
| [LocalTimeoutExecutor.cs](/src/BuildingBlocks/Resilience/Timeout/Internal/LocalTimeoutExecutor.cs#L44) | L44 | `"Resilience.ExecutionFailed"` (inline) |
| [LocalRetryExecutor.cs](/src/BuildingBlocks/Resilience/Retry/Internal/LocalRetryExecutor.cs#L60) | L60 | `"Resilience.RetryExhausted"` (inline) |
| [LocalFallbackHandler.cs](/src/BuildingBlocks/Resilience/Fallback/Internal/LocalFallbackHandler.cs#L37) | L37 | `"Resilience.FallbackFailed"` (inline) |

→ 🚩 **[CS.01]** `VKResilienceErrors` クラスに `static readonly VKError` 定数として抽出すべき。

#### CancellationToken 伝播 (CS.03) ✅

全 async メソッドチェーンで `CancellationToken` が途切れなく伝播されている。`OperationCanceledException` のフィルタリングも正しく実装。

#### Visibility 整合性 (AP.03) ✅

| レベル | 期待 | 実際 |
|:-------|:-----|:-----|
| Level 1 (Public) | `VK`/`IVK` prefix | ✅ |
| Level 2+ (Internal) | `internal` | ✅ |
| `Internal/` 内 | `internal sealed` | ✅ |

#### VKGuard (AP.01) 境界防御

⚠️ **指摘**: 以下のコンストラクタで `VKGuard` が未使用:

| ファイル | 行 | 問題 |
|:---------|:---|:-----|
| [LocalTimeoutExecutor.cs](/src/BuildingBlocks/Resilience/Timeout/Internal/LocalTimeoutExecutor.cs#L13-L16) | L13-16 | `_options = options;` → `_options = VKGuard.NotNull(options);` とすべき |
| [LocalRetryExecutor.cs](/src/BuildingBlocks/Resilience/Retry/Internal/LocalRetryExecutor.cs#L14-L18) | L14-18 | `_options = options;` → `_options = VKGuard.NotNull(options);` |
| [LocalCircuitBreaker.cs](/src/BuildingBlocks/Resilience/CircuitBreaker/Internal/LocalCircuitBreaker.cs#L26-L29) | L26-29 | `IsAllowed` の `key` パラメータは `string.IsNullOrEmpty` で検証しているが `VKGuard` 未使用 |
| [LocalRateLimiter.cs](/src/BuildingBlocks/Resilience/RateLimiting/Internal/LocalRateLimiter.cs#L24-L27) | L24-27 | 同上 |
| [LocalBulkhead.cs](/src/BuildingBlocks/Resilience/Bulkhead/Internal/LocalBulkhead.cs#L17-L20) | L17-20 | 同上 |

→ 🚩 **[AP.01]** コンストラクタ境界では `VKGuard.NotNull` の使用が必須。メソッド境界では `string.IsNullOrEmpty` の代わりに `VKGuard.NotNullOrWhiteSpace` を検討。

### 深度逻辑与状态演进审查

#### 脳内推演: Retry の成功→失敗フロー

1. **成功パス**: `action(cancellationToken)` 成功 → `VKResult.Success(result)` return ✅
2. **失敗パス**: 全リトライ消費 → `VKResult.Failure<T>(new VKError("Resilience.RetryExhausted", ...))` return ✅
3. **キャンセルパス**: `cancellationToken` キャンセル → `OperationCanceledException` re-throw ✅

→ 状態伝播は正確。

#### 逻辑死胡同の検出

1. ❌ **[ResilienceException.cs](/src/BuildingBlocks/Resilience/Common/Internal/ResilienceException.cs#L18-L21) L18-21]**: `innerException` パラメータが受け取られているが **基底クラスに渡されていない**。これはバグ。

```csharp
public ResilienceException(string message, Exception innerException)
    : base(DefaultCode, message)  // innerException が破棄されている！
{
}
```

2. ⚠️ **[IVKResiliencePipeline.cs](/src/BuildingBlocks/Resilience/Common/Protocols/IVKResiliencePipeline.cs)**: Pipeline インターフェースが定義されているが、**モジュール内に実装クラスが存在しない**。DI 登録もなし。消費者から見ると「宣言されたが使えない API」。

3. ⚠️ **[VKCircuitBreakerOptions.cs](/src/BuildingBlocks/Resilience/CircuitBreaker/VKCircuitBreakerOptions.cs) vs [LocalCircuitBreaker.cs](/src/BuildingBlocks/Resilience/CircuitBreaker/Internal/LocalCircuitBreaker.cs)**: Options に `FailureThreshold`, `DurationOfBreak`, `MinimumThroughput` が定義されているが、`LocalCircuitBreaker` は **これらの Options を DI で受け取っていない**。代わりにメソッドパラメータで `failureThreshold`, `cooldownDuration`, `failureRatio` を受け取っており、Options の値が使われていない。

#### 防御性逆向思考

**仮説**: 「CircuitBreaker の Options 設定が運用で無視される」

**証明**: `LocalCircuitBreaker` のコンストラクタは `VKCircuitBreakerOptions` を受け取らない（L21-24）。`RecordFailure` メソッドのデフォルト値 (`failureThreshold = 5`, `failureRatio = 0.5`) はハードコードされており、`appsettings.json` での設定変更が **全く反映されない**。これは運用チームの混乱を引き起こすリスクがある。

---

## 🚨 重大なアーキテクチャの懸念事項 (Critical Architectural Smells)

- ❌ **[Options-Implementation 不整合]**: [[LocalCircuitBreaker.cs](/src/BuildingBlocks/Resilience/CircuitBreaker/Internal/LocalCircuitBreaker.cs)] — `VKCircuitBreakerOptions` で定義された `FailureThreshold`, `DurationOfBreak`, `MinimumThroughput` が `LocalCircuitBreaker` のコンストラクタに注入されておらず、メソッドパラメータのデフォルト値がハードコードされている。設定ファイルの変更が反映されないため、運用時の制御不能リスクがある。

- ❌ **[innerException 破棄]**: [[ResilienceException.cs](/src/BuildingBlocks/Resilience/Common/Internal/ResilienceException.cs#L18-L21)] — `innerException` パラメータが基底クラス `VKBaseException` に渡されておらず、スタックトレースが失われる。デバッグ時の根本原因追跡が不可能になる。

---

## 🛡️ 非機能要件とセキュリティ (Non-Functional Requirements & Security)

- 🔒 **[メモリリーク懸念]**: [LocalCircuitBreaker.cs](/src/BuildingBlocks/Resilience/CircuitBreaker/Internal/LocalCircuitBreaker.cs) と [LocalRateLimiter.cs](/src/BuildingBlocks/Resilience/RateLimiting/Internal/LocalRateLimiter.cs) の `ConcurrentDictionary` はキーが無制限に蓄積される可能性がある。長期稼働アプリケーションでは、古い or 不使用のキーを削除する TTL/Eviction メカニズムが必要。

- 🔒 **[スレッドセーフティ]**: `LocalRateLimiter.IsAllowed` (L39-40) では `RemoveAll` + `Count` チェックを行うが、**`RecordRequest` が別スレッドで同時呼び出し**された場合、`IsAllowed` が `true` を返した後に `RecordRequest` が呼ばれる前に他のリクエストが許可される TOCTOU レースコンディションが存在する。

---

## 🧪 テスト容易性と疎結合性 (Testability & Decoupling)

- ⚙️ **[テスト容易性: 良好]**: 全実装が `IVK*` インターフェース経由でモック可能。`TimeProvider` 注入により時間依存の挙動をテスト可能。`VKResult<T>` パターンにより例外ベースのテストが不要。

- ⚙️ **[改善提案]**: `LocalRetryExecutor.CalculateDelay` (L114) で `Random.Shared` を直接使用している。ジッター計算の決定論的テストのため、乱数ソースの注入を検討すべき。

---

## 🔭 可観測性の準拠度 (Observability Readiness)

- 📡 **[メトリクス: 良好]**: `ResilienceDiagnostics` にて `Counter<long>` (`StrategyExecutionCount`) と `ActivitySource` が定義済。`VKResilienceDiagnosticsConstants` でセマンティックトークン化済。

- 📡 **[ロギング: 不足]**: `[LoggerMessage]` SG ベースのログメッセージが未定義。Retry の試行数、CircuitBreaker のトリップ、Timeout 発生等の重要なイベントがログに記録されない。運用時のトラブルシューティングに支障。

- 📡 **[メトリクス未活用]**: `ResilienceDiagnostics.RecordStrategyExecution` が定義されているが、各 `Local*` 実装から **呼び出されていない**。メトリクス収集が実質的に機能していない。

---

## ⚠️ コード品質とコーディング規約のリスク (Code Quality & Standard Risks)

- ⚠️ **[CS.01 エラー定数]**: エラーコード文字列がインラインで使用されている（`"Resilience.Timeout"`, `"Resilience.RetryExhausted"` 等）。`VKResilienceErrors` 定数クラスに抽出すべき。

- ⚠️ **[AP.01 VKGuard]**: 複数のコンストラクタで `VKGuard.NotNull` が未使用。防御的プログラミングの一貫性が不足。

- ⚠️ **[IVKResiliencePipeline 未実装]**: パブリックインターフェースが定義されているが、実装が存在しない。消費者に誤解を与える可能性がある。

---

## ✅ 評価ポイント (Highlights / Good Practices)

1. **垂直スライス構造の完全準拠**: 6つのFeatureが統一されたパターン（Options + Interface + Internal実装 + Feature登録）で構成され、非常に高い一貫性を持つ。

2. **SG自動化の活用**: `[VKBlockMarker]`, `[VKFeature]`, `[VKBlockDiagnostics]` の3つの Source Generator を完全活用し、ボイラープレートの手書きを排除。

3. **`VKResult<T>` パターンの徹底**: 全ての async 操作が `VKResult<T>` を返し、例外フローを排除。`OperationCanceledException` の適切なフィルタリング。

4. **`Func<T, T>` 変換パターン (ADR-016)**: SG-generated registration が不変 Options の関数型変換に完全対応。

5. **外部依存ゼロ**: Polly 等の外部レジリエンスライブラリに依存せず、軽量かつ自己完結型の実装。

6. **`TimeProvider` による非確定性排除**: `LocalRetryExecutor`, `LocalCircuitBreaker`, `LocalRateLimiter` で `TimeProvider` を注入し、テスタブルな時間制御を実現。

7. **Feature-level Validation**: 各 `*Feature.cs` で `ValidateFeatureCustom` を実装し、Options のクロスプロパティバリデーションを提供。

---

## 💡 改善ロードマップ (Evolutionary Roadmap)

### 1. 最優先対応 (Immediate Action)

| # | 対象 | アクション |
|:--|:-----|:---------|
| 1 | [ResilienceException.cs](/src/BuildingBlocks/Resilience/Common/Internal/ResilienceException.cs#L18-L21) | `innerException` を `base(DefaultCode, message, innerException)` に渡す（バグ修正） |
| 2 | [LocalCircuitBreaker.cs](/src/BuildingBlocks/Resilience/CircuitBreaker/Internal/LocalCircuitBreaker.cs) | コンストラクタに `VKCircuitBreakerOptions` を注入し、デフォルト値を Options から取得 |
| 3 | 全 `Local*` 実装 | コンストラクタに `VKGuard.NotNull` を追加 (AP.01) |

### 2. リファクタリング提案 (Refactoring)

| # | 対象 | アクション |
|:--|:-----|:---------|
| 4 | 新規: `VKResilienceErrors.cs` | エラー定数を `static class` に抽出 (CS.01) |
| 5 | 新規: `ResilienceLogs.cs` | `[LoggerMessage]` SG で Retry/Timeout/CircuitBreaker イベントログを追加 (OR.01) |
| 6 | 各 `Local*` 実装 | `ResilienceDiagnostics.RecordStrategyExecution` を呼び出しに組み込む |
| 7 | `IVKResiliencePipeline` | 実装クラスを作成するか、未使用のため削除を検討 |
| 8 | `LocalRateLimiter` / `LocalCircuitBreaker` | ConcurrentDictionary に TTL/Eviction メカニズムを追加 |

### 3. 推奨される学習トピック (Learning Suggestions)

- **Polly v8 との比較**: 現在の軽量実装と Microsoft.Extensions.Resilience (Polly v8) のアーキテクチャ比較を行い、将来の統合方針を決定
- **分散レジリエンス**: 現在の In-Memory 実装から Redis-backed CircuitBreaker / RateLimiter へのスケーリング戦略
