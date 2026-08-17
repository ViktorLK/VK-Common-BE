# 🏛️ アーキテクチャ監査レポート: AI.Synapse

**モジュール**: `VK.Blocks.AI.Synapse`
**監査日**: 2026-08-14
**監査者**: VK.Blocks Lead Architect (Strict Mode)
**対象パス**: `/src/BuildingBlocks/AI.Synapse/`
**Handshake**: `Active: [L1+L2:AI.Synapse] | Context: AI.Synapse | Sync: [AP.03:Ctx, BB.01:Ctx, BB.02:Ctx, BB.03:Ctx, BB.07:Ctx, DL.01:Ctx, BB.04:L3, BB.05:L3, AP.02:L3, CS.02:L3, CS.01:L3, AP.01:L3]`
**Audit Load**: `[BB.01:Ctx, AP.03:Ctx, BB.02:Ctx, BB.03:Ctx, BB.04:L3, BB.05:L3, AP.02:L3, CS.02:L3, CS.01:L3, AP.01:L3] | Status: Verified ✅`

---

## 📊 監査サマリー (Audit Summary)

- **総合スコア**: **82/100**
- **Fast Audit スコア**: 25.5/27 (94%)
- **対象レイヤー判定**: BuildingBlock Infrastructure Layer — AI マルチプロバイダ・ルーティング＆レジリエンスゲートウェイ
- **総評 (Executive Summary)**:

AI.Synapse は VK.Blocks アーキテクチャの標準に高度に準拠した、成熟度の高いモジュールである。Vertical Slice レイアウト（Routing / Quota / Cost）は明確に分離され、`[VKBlockMarker]` + `[VKFeature]` による SG 自動化パイプライン、`VKResult<T>` パターンの徹底、`VKGuard` による境界防御、`TimeProvider` / `IVKGuidGenerator` による CS.06 準拠、`[LoggerMessage]` SG + `[VKBlockDiagnostics]` による可観測性基盤など、Industrial DNA の主要要件をほぼ完全に満たしている。

ただし、以下の3つの重要な問題が確認された：
1. **LocalAICircuitBreaker / LocalAIRateLimiter のコンストラクタで VKGuard 未使用** — AP.01 境界防御違反
2. **DefaultAIRouteDispatcher.TryRecordUsageAndCost で fire-and-forget Task 未await** — CS.03 async 規律違反
3. **csproj の `RootNamespace` が `VK.Blocks.AI.Gateway` と旧名残** — 命名不整合リスク

---

## ⚡ Fast Audit: AI.Synapse

**Date**: 2026-08-14 | **Score**: 25.5/27 (94%)

### 📁 Structure (BB.01)
- ✅ S-01: `Common/` + Feature Slices (`Routing/`, `Quota/`, `Cost/`) — Vertical Slice 構造あり
- ✅ S-02: SG パイプライン (`[VKBlockMarker]`) による自動 DI 生成
- ✅ S-03: `Common/Diagnostics/` + `Common/Diagnostics/Internal/` — 診断基盤あり
- ✅ S-04: `Common/Diagnostics/Internal/` — 内部診断クラスあり
- ✅ S-05: [`VKAISynapseBlock.cs`](/src/BuildingBlocks/AI.Synapse/VKAISynapseBlock.cs) — モジュールルートにマーカー配置
- ✅ S-06: Options 各 Feature Slice に配置 (`VKRoutingOptions.cs`, `VKQuotaOptions.cs`, `VKCostOptions.cs`)

### 🏷️ Marker (BB.02)
- ✅ M-01: `[VKBlockMarker(Dependencies = [typeof(VKAIBlock), typeof(VKResilienceBlock)], Toggleable = false)]` — 属性使用確認
- ✅ M-02: レガシー `IVKBlockMarker` 手動実装なし — Pass
- ✅ M-03: `public sealed partial class VKAISynapseBlock` — `sealed partial class` 宣言
- ✅ M-04: `Dependencies = [typeof(VKAIBlock), typeof(VKResilienceBlock)]` — 依存関係明示

### 🔌 DI Registration (BB.03, AP.02/04)
- ✅ D-01: `[VKBlockMarker]` SG により `IsVKBlockRegistered` 自動生成
- ✅ D-02: `[VKBlockMarker]` SG により `AddVKBlockMarker` 自動生成
- ✅ D-03: `[VKBlockMarker]` SG により `AddVKBlockOptions` 自動生成
- ✅ D-04: `TryAddSingleton` 全 Feature (14 箇所) で使用確認
- ✅ D-05: 直接 `services.AddSingleton/Scoped/Transient` 検出なし
- ✅ D-06: SG により `BlockRegistration.Register` 自動生成
- ✅ D-07: SG により `AddVKAISynapseBlock` ラッパー自動生成

### ⚙️ Options (BB.05, AP.04)
- ✅ O-01: 全 Options が `sealed partial record ... : IVKBlockOptions` パターン (3 件)
- ✅ O-02: `VK` プレフィックス: `VKRoutingOptions`, `VKQuotaOptions`, `VKCostOptions`
- ✅ O-03: `SectionName` は `[VKFeature]` SG により自動生成
- ✅ O-04: レガシー `sealed class ... IVKBlockOptions` 検出なし

### 🔍 Implementation Patterns (CS.01/03/06, OR.01, AP.01, BB.04)
- ✅ I-01: `public sealed` = 8, `public class` (unsealed) = 0 — 完全準拠 (100%)
- ✅ I-02: `VKGuard.` = 21 箇所 — 境界防御広範使用
- ✅ I-03: `ConfigureAwait(false)` = 6, `await` = 6 — 100% カバレッジ
- ✅ I-04: `[LoggerMessage]` = 4 定義 — SG ログ使用
- ✅ I-05: 直接 `.LogInformation()` 等のレガシー呼出し = 0 — 完全準拠
- ✅ I-06: `[VKBlockDiagnostics<VKAISynapseBlock>]` — 診断属性確認
- ✅ I-07: `VKResult<T>` / `VKResult.Failure` 広範使用 (20+ 箇所)
- ✅ I-08: `DateTime.UtcNow` / `DateTime.Now` = 0 — `TimeProvider` 使用確認
- ✅ I-09: `Guid.NewGuid()` = 0 — `IVKGuidGenerator` 使用確認
- ✅ I-10: `JsonSerializer.Serialize/Deserialize` = 0 — 該当なし (N/A)
- ✅ I-11: `Microsoft.EntityFrameworkCore` / `StackExchange.Redis` = 0 — 依存汚染なし

### 📛 Naming & Visibility (AP.03)
- ✅ N-01: パブリック型はすべて `VK` / `IVK` プレフィックス使用
- ✅ N-02: `Internal/` 配下 14 クラスすべて `internal sealed` 宣言
- ⚠️ N-03: `VK.Blocks.AI.Synapse.csproj` の `<RootNamespace>` が `VK.Blocks.AI.Gateway` — 旧名残で不整合

### 📊 Summary Table

| Category | Tier | ✅ | ❌ | ⚠️ |
| :--- | :--- | :--- | :--- | :--- |
| Structure | 🟡 | 6 | 0 | 0 |
| Marker | 🔴 | 4 | 0 | 0 |
| DI Registration | 🔴 | 7 | 0 | 0 |
| Options | 🟡 | 4 | 0 | 0 |
| Implementation | 🔴 | 11 | 0 | 0 |
| Naming | 🟡 | 2 | 0 | 1 |
| **Total** | | **34** | **0** | **1** |

### 🚩 Audit Exceptions
Audit: ⚠️ [AP.03] `RootNamespace` in csproj is `VK.Blocks.AI.Gateway` — stale artifact from naming migration. Namespace declarations in source code are correct (`VK.Blocks.AI.Synapse`).

---

## 🔍 Phase 2: DI 登録監査 (Registration Audit)

### [VKBlockMarker] SG パイプライン検証

本モジュールは `[VKBlockMarker]` + `[VKFeature]` Source Generator による完全自動 DI パイプラインを採用している。手動 `DependencyInjection/` フォルダは存在せず、すべて SG が生成する。

| Check | Rule | Tier | Result | Evidence |
|:------|:-----|:-----|:-------|:---------|
| Execution Order | BB.03 | 🔴 | ✅ Pass | SG 自動生成（Check-Self → Options → Mark-Self → Validator → Toggle → Custom Hook） |
| Func Transform | BB.03 | 🔴 | ✅ Pass | SG 自動生成（`Func<T,T>` パターン） |
| Enabled Policy Position | BB.03 | 🔴 | ✅ Pass | `Toggleable = false` のためスキップ合法 |
| Builder Pattern | BB.03 | 🟡 | ✅ Pass | `IVKAISynapseBuilder` SG 生成 + `TryAdd` 拡張メソッド使用 |
| OptionsValidator Quality | BB.05 | 🔴 | ✅ Pass | `ValidateFeatureCustom` でクリティカルプロパティ検証あり (`MaxFallbackAttempts > 0`, `DefaultCircuitBreakerThreshold > 0`, `DefaultMaxConcurrency > 0`) |

### Feature Registration Details

| Feature | Marker | Registration | Validation |
|:--------|:-------|:-------------|:-----------|
| [`RoutingFeature.cs`](/src/BuildingBlocks/AI.Synapse/Routing/RoutingFeature.cs) | `[VKFeature(typeof(VKAISynapseBlock), OptionsType = typeof(VKRoutingOptions))]` | 4 services (`TryAddSingleton`) | `MaxFallbackAttempts > 0` ✅ |
| [`QuotaFeature.cs`](/src/BuildingBlocks/AI.Synapse/Quota/QuotaFeature.cs) | `[VKFeature(typeof(VKAISynapseBlock), OptionsType = typeof(VKQuotaOptions))]` | 5 services (`TryAddSingleton`) | `DefaultCircuitBreakerThreshold > 0`, `DefaultMaxConcurrency > 0` ✅ |
| [`CostFeature.cs`](/src/BuildingBlocks/AI.Synapse/Cost/CostFeature.cs) | `[VKFeature(typeof(VKAISynapseBlock), OptionsType = typeof(VKCostOptions))]` | 1 service (`TryAddSingleton`) | No-op (empty body) ✅ |

**Phase 2 判定**: ✅ **PASS** — SG 自動生成パイプラインにより BB.03 実行順序は構造的に保証されている。

---

## 🔍 Phase 3: 実装監査 (Implementation Deep Audit)

### 1. 設計原則 (Design Principles) — SOLID/KISS/YAGNI/DRY

**評価: 優秀 (9/10)**

- **SRP**: 各 Feature Slice（Routing / Quota / Cost）が明確な単一責任を持ち、Common レイヤーは横断的関心事のみを扱う。
- **DIP**: すべての実装は公開インターフェース（`IVKAIRouter`, `IVKAIProviderTracker`, `IVKAICostCalculator` 等）に依存し、具象クラスへの直接依存は皆無。
- **OCP**: `TryAddSingleton` パターンにより、消費者は独自実装を DI で差し替え可能。
- **KISS**: `DefaultAIRouter` の Strategy switch 文は 3 ケースのみで過度な抽象化を避けている。

### 2. 設計パターン (Design Patterns)

| Pattern | Usage | Appropriateness |
|:--------|:------|:----------------|
| **Strategy** | `VKAIRoutingStrategy` enum + `DefaultAIRouter.ResolveCandidatesAsync` 内 switch 分岐 | ✅ 適切 — 現在の 3 戦略に最適 |
| **Factory** | `IVKAISynapseModelFactory` / `DefaultAISynapseModelFactory` | ✅ 適切 — CS.06 準拠の非確定 API 注入点 |
| **Composite/Mediator** | `DefaultAIProviderTracker` が CircuitBreaker + RateLimiter + MetricsCollector を統合 | ✅ 適切 — Facade パターンで複合状態追跡を簡素化 |
| **Circuit Breaker** | `LocalAICircuitBreaker` → `IVKCircuitBreaker` (Resilience BB) | ✅ 適切 — 既存 Resilience BB への委譲 |
| **Token Bucket** | `LocalAITokenBudgetManager.TokenBucket` — 内部クラスによるスライディングウィンドウ | ✅ 適切 — 簡潔な TPM 制限実装 |

### 3. アーキテクチャ原則 (Architectural Principles)

- **関心事分離**: Routing（ルート選択 + フォールバック）、Quota（回路遮断 + レート制限 + トークンバジェット）、Cost（コスト計算）の三層分離は秀逸。
- **カプセル化**: すべての実装クラスが `internal sealed` で外部からアクセス不可。
- **凝集性**: 各 Feature Slice は自己完結的な Protocols / Internal / Models 構成。
- **結合度**: Feature 間の直接依存は最小限（DispatcherのみがCostCalculatorとTokenBudgetManagerをオプショナル注入）。

### 4. アーキテクチャスタイル (Architectural Styles)

- **Vertical Slice Architecture**: 完全準拠。各 Feature が独立した Registration + Options + Protocols + Internal 構成を持つ。
- **Clean Architecture**: Domain 抽象（公開インターフェース）と Infrastructure 実装（Internal）の分離が徹底。

### 5. アーキテクチャパターン (Architectural Patterns)

- **Anti-Corruption Layer**: `IVKCircuitBreaker`, `IVKBulkhead`, `IVKRateLimiter` など Resilience BB のプリミティブを AI Synapse 独自のドメインインターフェース (`IVKAICircuitBreaker`, `IVKAIRateLimiter`) で包み、AI 固有セマンティクスに変換。正当なパターン適用。

### 6. エンタープライズパターン (Enterprise Patterns)

- **Circuit Breaker**: ✅ `IVKCircuitBreaker` 委譲による接続レベルの回路遮断
- **Rate Limiting**: ✅ Bulkhead (並行性) + RPM (スループット) のデュアル制限
- **Token Budget**: ✅ テナント/プロバイダー単位の TPM スライディングウィンドウ制限
- **Metrics & Observability**: ✅ OTel Counter/Histogram + [LoggerMessage] SG による構造化ログ
- **Multi-Tenant Isolation**: ✅ `IVKTenantScoped` + `IVKIdentityContext.TenantId` による接続スコーピング
- **Idempotent Registration**: ✅ `TryAddSingleton` + `[VKBlockMarker]` SG

### 7. VK.Blocks 固有の準拠度 (VK.Blocks Compliance — Deep)

| 項目 | 判定 | 詳細 |
|:-----|:-----|:-----|
| **BB.03 実行順序** | ✅ | SG 自動生成により構造的保証 |
| **ADR-016 Func 変換** | ✅ | SG 自動生成により `Func<T,T>` パターン |
| **Error 定数パターン** | ✅ | [`VKAISynapseErrors.cs`](/src/BuildingBlocks/AI.Synapse/Common/Errors/VKAISynapseErrors.cs) に `static readonly VKError` 6 定数。`AISynapse.{Category}` 形式 |
| **CancellationToken 伝播** | ✅ | 全 async チェーンで `CancellationToken` パラメータ → `ConfigureAwait(false)` 100% |
| **Visibility** | ✅ | パブリック型: `VK`/`IVK` プレフィックス、`Internal/`: `internal sealed` |
| **VKGuard 境界防御** | ⚠️ | 一部コンストラクタで `VKGuard` 未使用（後述） |
| **TimeProvider (CS.06)** | ✅ | `LocalAIMetricsCollector`, `LocalAITokenBudgetManager`, `DefaultAISynapseModelFactory` で使用 |
| **IVKGuidGenerator (CS.06)** | ✅ | `DefaultAISynapseModelFactory` で使用 |
| **Result パターン (CS.01)** | ✅ | 全 async メソッドが `VKResult<T>` 返却。raw string 不使用 |

---

## 🚨 重大なアーキテクチャの懸念事項 (Critical Architectural Smells)

### ❌ **[CS.03/非同期規律] Fire-and-forget Task in DefaultAIRouteDispatcher**

[`DefaultAIRouteDispatcher.cs:L175`](/src/BuildingBlocks/AI.Synapse/Routing/Internal/DefaultAIRouteDispatcher.cs#L175)

```csharp
_ = _tokenBudgetManager.RecordUsageAsync(tenantId, (int)Math.Min(totalTokens, int.MaxValue));
```

`RecordUsageAsync` は `Task<VKResult>` を返すが、`_ =` で戻り値を破棄している。これにより：

1. **例外の黙殺**: `RecordUsageAsync` が内部で例外をスローした場合、Unobserved Task Exception となり、ログにも記録されずデバッグが困難になる。
2. **トークン予算の不整合**: 記録が失敗してもフォールバック処理がないため、スライディングウィンドウのカウンタが実際の消費量と乖離し、TPM 制限が不正確になるリスク。
3. **CancellationToken 未伝播**: 外側の `cancellationToken` が渡されていないため、リクエストキャンセル時にも不要な処理が続行される。

**修正案**: `await` するか、最低限 `Task.Run` + try/catch + ログで安全にバックグラウンド実行する。

---

## 🛡️ 非機能要件とセキュリティ (Non-Functional Requirements & Security)

### 🔒 **[AP.01/VKGuard] コンストラクタ境界防御の不足**

以下のクラスのコンストラクタで、必須依存関係に `VKGuard.NotNull()` が使用されていない：

| Class | File | Missing Guards |
|:------|:-----|:---------------|
| `LocalAICircuitBreaker` | [`LocalAICircuitBreaker.cs:L15-21`](/src/BuildingBlocks/AI.Synapse/Quota/Internal/LocalAICircuitBreaker.cs#L15-L21) | `circuitBreaker`, `defaults` |
| `LocalAIRateLimiter` | [`LocalAIRateLimiter.cs:L13-21`](/src/BuildingBlocks/AI.Synapse/Quota/Internal/LocalAIRateLimiter.cs#L13-L21) | `bulkhead`, `rateLimiter`, `defaults` |
| `DefaultAIProviderTracker` | [`DefaultAIProviderTracker.cs:L13-21`](/src/BuildingBlocks/AI.Synapse/Quota/Internal/DefaultAIProviderTracker.cs#L13-L21) | `circuitBreaker`, `rateLimiter`, `metricsCollector` |

これらのクラスは DI により注入されるため NullReferenceException の即時リスクは低いが、**AP.01 規約違反** であり、テスト時やモック構成ミス時のデバッグ困難性を高める。

### 🔒 **[命名不整合] csproj RootNamespace の旧名残**

[`VK.Blocks.AI.Synapse.csproj:L3`](/src/BuildingBlocks/AI.Synapse/VK.Blocks.AI.Synapse.csproj#L3)

```xml
<RootNamespace>VK.Blocks.AI.Gateway</RootNamespace>
```

ソースコード内の実際の namespace は `VK.Blocks.AI.Synapse` で統一されているが、csproj の `RootNamespace` が旧名 `VK.Blocks.AI.Gateway` のままである。現時点でビルドには影響しないが、新規ファイル追加時に IDE が旧 namespace を自動挿入するリスクがある。

### 🔒 **[パフォーマンス] DefaultAIRouter の LINQ ToList() チェーン**

[`DefaultAIRouter.cs:L31-63`](/src/BuildingBlocks/AI.Synapse/Routing/Internal/DefaultAIRouter.cs#L31-L63)

`ResolveCandidatesAsync` 内で `pool.ToList()` → `Where().ToList()` → `OrderBy().ToList()` と 3 回の List 生成が行われる。プロバイダー数が通常 10 以下であるため即座のパフォーマンス問題ではないが、大規模プール環境では最適化の余地がある。

---

## 🧪 テスト容易性と疎結合性 (Testability & Decoupling)

### ⚙️ **テスト容易性**: 極めて高い

- **すべての依存関係がインターフェース注入** — モック容易性が完全に確保されている。
- **`new` キーワードの使用**: 内部 DTO (`List<T>`, `ConcurrentDictionary`, `TagList`) のインスタンス化のみ — 外部依存の具象生成なし。
- **TimeProvider 注入**: `LocalAIMetricsCollector`, `LocalAITokenBudgetManager` は `TimeProvider` をコンストラクタ注入で受け取り、テスト時の時刻制御が可能。
- **Optional 依存**: `IVKAICostCalculator?`, `IVKAITokenBudgetManager?`, `IVKAIConnectionStore?`, `ILogger?` がすべてオプショナルであり、最小構成でのユニットテストが可能。

### ⚙️ **デカップリング品質**: 良好

- **Resilience BB への委譲**: `IVKCircuitBreaker`, `IVKBulkhead`, `IVKRateLimiter` を直接利用するのではなく、AI Synapse 独自のラッパーインターフェース (`IVKAICircuitBreaker`, `IVKAIRateLimiter`) で包んでいる。
- **AI BB への依存**: `VKAIProviderType`, `VKAIModelIds`, `IVKAIProviderOptions` は親ブロック (`VK.Blocks.AI`) の型を使用 — 正当な依存方向（子→親）。

---

## 🔭 可観測性の準拠度 (Observability Readiness)

### 📡 **運用監視**: 優秀

| 要素 | 実装状況 |
|:-----|:---------|
| **Result\<T\> エラーハンドリング** | ✅ 全 async メソッドが `VKResult<T>` 返却、RFC 7807 準拠エラー定数 |
| **構造化ログ** | ✅ `[LoggerMessage]` SG × 4 定義（RequestRouted, ProviderFailedFallback, RateLimitExceeded, RequestCompleted） |
| **OTel Metrics** | ✅ `Counter<long>` × 3 + `Histogram<double>` × 1 + `Counter<double>` × 1 |
| **ActivitySource** | ✅ `[VKBlockDiagnostics<VKAISynapseBlock>]` により `Source` / `Meter` 自動生成 |
| **Diagnostics Constants** | ✅ [`VKAISynapseDiagnosticsConstants`](/src/BuildingBlocks/AI.Synapse/Common/Diagnostics/VKAISynapseDiagnosticsConstants.cs) に 6 つのセマンティックトークン定義 |
| **TraceId 伝播** | ✅ `Stopwatch` + `RecordRequest` タグ付きメトリクスで request-level トレーシング |

---

## ⚠️ コード品質とコーディング規約のリスク (Code Quality & Standard Risks)

### ⚠️ **[防御的プログラミング] null チェックの不統一**

Quota 内部実装（`LocalAICircuitBreaker`, `LocalAIRateLimiter`, `DefaultAIProviderTracker`, `LocalAIMetricsCollector`）では、メソッド引数の `connection` に対して `if (connection is null) return ...` パターンを手動で実装しているが、コンストラクタの必須依存関係には `VKGuard` を使用していない。

AP.01 の「**ALL method and constructor boundaries MUST use VKGuard**」に照らし合わせると、コンストラクタでの `VKGuard` 未使用は規約違反である。

一方、メソッド引数の `connection` null チェックには `if (connection is null)` を使用しているが、これは `VKGuard.NotNull` ではなく「graceful return」パターンであり、void メソッドで例外をスローしたくないケースとして設計意図が理解できる。ただし、AP.01 厳密解釈では `VKGuard` が望ましい。

### ⚠️ **[LatencyOptimized 戦略の未実装]**

[`VKAIRoutingStrategy.cs:L26`](/src/BuildingBlocks/AI.Synapse/Routing/Models/VKAIRoutingStrategy.cs#L26) で `LatencyOptimized = 3` が定義されているが、[`DefaultAIRouter.cs`](/src/BuildingBlocks/AI.Synapse/Routing/Internal/DefaultAIRouter.cs) の switch 文には該当ケースがなく、`default` ブランチ（= `Preference`）にフォールスルーする。定義された enum 値に対応する実装がないことは、消費者の期待と動作の乖離を招くリスクがある。

### ⚠️ **[DefaultAICostCalculator] ハードコードされた料金テーブル**

[`DefaultAICostCalculator.cs:L20-25`](/src/BuildingBlocks/AI.Synapse/Cost/Internal/DefaultAICostCalculator.cs#L20-L25) にベースライン料金がハードコードされている。モデル料金は頻繁に変更されるため、`VKCostOptions.CustomPricing` でのオーバーライドは提供されているが、デフォルト料金の更新にはコード変更が必要。

---

## ✅ 評価ポイント (Highlights / Good Practices)

1. **Industrial DNA 完全適用**: `[VKBlockMarker]` + `[VKFeature]` SG パイプラインにより、DI 登録・Options 検証・Block マーカーがすべて自動化。手動 DI フォルダの完全排除に成功。
2. **CS.06 完全準拠**: `TimeProvider`, `IVKGuidGenerator` を `DefaultAISynapseModelFactory` 経由で注入し、非確定 API の直接呼出しをゼロに削減。
3. **Resilience BB との正当な Anti-Corruption Layer**: `IVKCircuitBreaker` → `IVKAICircuitBreaker` の変換により、AI ドメイン固有セマンティクス（`VKAIConnection` ベースの操作）を自然に表現。
4. **マルチテナント対応**: `IVKTenantScoped` + `VKTenantId` フィルタリングにより、接続プールのテナント分離が設計レベルで保証。
5. **Optional 依存の適切な設計**: `IVKAICostCalculator?`, `IVKAITokenBudgetManager?` をオプショナル注入とすることで、最小構成でのモジュール利用が可能。AP.07 (Non-intrusive capability) に準拠。
6. **VKSensitiveString による API キー保護**: `VKAIConnection.ApiKey` が `VKSensitiveString?` 型であり、ログ出力時の PII マスキングが構造的に保証。
7. **Diagnostics の充実**: OTel Counter/Histogram + LoggerMessage SG + DiagnosticsConstants による完全な可観測性基盤。

---

## 💡 改善ロードマップ (Evolutionary Roadmap)

### 1. 最優先対応 (Immediate Action)

| Priority | Issue | Rule | Action |
|:---------|:------|:-----|:-------|
| 🔴 P1 | Fire-and-forget `RecordUsageAsync` | CS.03 | `await` に変更するか、安全なバックグラウンド実行パターンに置換。[`DefaultAIRouteDispatcher.cs:L175`](/src/BuildingBlocks/AI.Synapse/Routing/Internal/DefaultAIRouteDispatcher.cs#L175) |
| 🔴 P2 | コンストラクタ `VKGuard` 不足 | AP.01 | `LocalAICircuitBreaker`, `LocalAIRateLimiter`, `DefaultAIProviderTracker` の 3 クラスのコンストラクタに `VKGuard.NotNull()` を追加。 |
| 🟡 P3 | csproj `RootNamespace` 旧名残 | AP.03 | `<RootNamespace>VK.Blocks.AI.Gateway</RootNamespace>` → 行を削除するか `VK.Blocks.AI.Synapse` に修正。 |

### 2. リファクタリング提案 (Refactoring)

| Priority | Issue | Action |
|:---------|:------|:-------|
| 🟡 R1 | `LatencyOptimized` 戦略の未実装 | `DefaultAIRouter` に `LatencyOptimized` ケースを追加し、`IVKAIMetricsCollector.GetAverageLatencyMs()` を活用した実装を提供。 |
| 🟡 R2 | `DefaultAIRouter` の LINQ 最適化 | `pool.ToList()` → `pool as IReadOnlyList<T> ?? pool.ToList()` による不要な List コピーの回避。 |
| 🟡 R3 | メソッド引数 `null` チェックの `VKGuard` 統一 | Quota 内部実装のメソッド引数 `connection is null` パターンを `VKGuard.NotNull(connection)` に統一（void 返却メソッドは要設計判断）。 |

### 3. 推奨される学習トピック (Learning Suggestions)

- **Polly v8 + Microsoft.Extensions.Resilience**: 将来的に `DefaultAIRouteDispatcher` の retry/fallback ロジックを Polly パイプラインに統合することで、より宣言的かつテスト容易なレジリエンス制御が可能。
- **Keyed Services (DI)**: .NET 8+ の `FromKeyedServices` を活用した Provider 別インスタンス管理の検討。

---

**Audit**: 🚩 [CS.03] Fire-and-forget Task `RecordUsageAsync` — unobserved exception risk | 🚩 [AP.01] Missing VKGuard in 3 constructors

✅ Full Audit complete. Saved to: `docs/04-AuditReports/AI.Synapse/AI.Synapse_20260814.md`
Phase 1: 25.5/27 (94%) | Phase 2: PASS | Phase 3 Score: 82/100
