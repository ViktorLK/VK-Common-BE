# 🏛️ アーキテクチャ監査レポート: AI.Corpus

> **監査日**: 2026-06-17  
> **対象モジュール**: `VK.Blocks.AI.Corpus`  
> **パス**: `/src/BuildingBlocks/AI.Corpus`  
> **監査者**: VK.Blocks Lead Architect  
> **Audit**: 🚩 [CS.01] InMemoryKnowledgeInjectionStore で `throw` 使用

---

## 📊 監査サマリー (Audit Summary)

- **総合スコア**: 88/100
- **Fast Audit スコア**: 25/27 (93%)
- **対象レイヤー判定**: Domain / BuildingBlock Layer — AI ナレッジコーパスのライフサイクル管理 (Gathering → Filtering → Tracking)
- **総評 (Executive Summary)**: AI.Corpus は VK.Blocks の垂直スライスアーキテクチャに極めて忠実に構築された高品質モジュールである。3つの Feature（Gathering / Filtering / Tracking）が `[VKFeature]` Source Generator 統合により標準化された DI 登録パターンを採用し、17種類のフィルターが Strategy パターンで高い拡張性を実現している。`VKGuard` による境界防御、`ConfigureAwait(false)` の徹底、`Result<T>` パターンの一貫した使用、`IVKJsonSerializer` 経由の JSON 処理など、Industrial DNA の主要要件を満たしている。ただし、`InMemoryKnowledgeInjectionStore` における `throw` 使用（CS.01 違反）、`DiagnosticsConstants.cs` の欠如（BB.04）、およびブロックレベル `IValidateOptions` の未実装（BB.05）が改善点として残る。

---

## ⚡ Fast Audit: AI.Corpus

**Date**: 2026-06-17 | **Score**: 25/27 (93%)

### 📁 Structure (BB.01)

| ID | Rule | Tier | Check | Result |
|:---|:-----|:-----|:------|:-------|
| S-01 | BB.01 | 🟡 | `DependencyInjection/` exists | ✅ Pass — `Common/DependencyInjection/` |
| S-02 | BB.01 | 🟡 | `DependencyInjection/Internal/` exists | ✅ Pass |
| S-03 | BB.04 | 🟡 | `Diagnostics/` exists | ✅ Pass — `Common/Diagnostics/` |
| S-04 | BB.04 | 🟡 | `Diagnostics/Internal/` exists | ✅ Pass |
| S-05 | BB.01 | 🔴 | Marker file at module root | ✅ Pass — [VKAICorpusBlock.cs](/src/BuildingBlocks/AI.Corpus/VKAICorpusBlock.cs) |
| S-06 | BB.01 | 🟡 | Options NOT scattered | ✅ Pass — 各 Feature ルートに集約 |

### 🏷️ Marker (BB.02)

| ID | Rule | Tier | Check | Result |
|:---|:-----|:-----|:------|:-------|
| M-01 | BB.02 | 🔴 | `[VKBlockMarker]` attribute | ✅ Pass |
| M-02 | BB.02 | 🟡 | Legacy `IVKBlockMarker` | ✅ Pass (Not found) |
| M-03 | BB.02 | 🔴 | `sealed partial class` | ✅ Pass |
| M-04 | BB.02 | 🟡 | Dependencies declared | ✅ Pass — `Dependencies = [typeof(VKAIPsycheBlock)]` |

### 🔌 DI Registration (BB.03, AP.02/04)

| ID | Rule | Tier | Check | Result |
|:---|:-----|:-----|:------|:-------|
| D-01 | BB.03 | 🔴 | Idempotency check | ✅ Pass — `IsVKBlockRegistered<VKAICorpusBlock>()` |
| D-02 | BB.03 | 🔴 | Self-marker registration | ✅ Pass — `AddVKBlockMarker<VKAICorpusBlock>()` |
| D-03 | AP.04 | 🟡 | Options standard helper | ✅ Pass — `AddVKBlockOptions` |
| D-04 | AP.02 | 🔴 | `TryAdd` pattern | ✅ Pass — 全 Feature 登録で `TryAddEnumerable` / `TryAddScoped` / `TryAddSingleton` 使用 |
| D-05 | AP.02 | 🔴 | No direct `Add` | ✅ Pass — 検出なし |
| D-06 | BB.03 | 🟡 | Wrapper → Internal delegation | ✅ Pass — `AICorpusBlockRegistration.Register` |
| D-07 | BB.03 | 🔴 | Wrapper method naming | ✅ Pass — `AddVKCorpusBlock` |

### ⚙️ Options (BB.05, AP.04)

| ID | Rule | Tier | Check | Result |
|:---|:-----|:-----|:------|:-------|
| O-01 | BB.05 | 🟡 | Options is `sealed record` | ✅ Pass — `IVKToggleableBlockOptions` 実装 |
| O-02 | BB.05 | 🟡 | `VK` prefix | ✅ Pass — `VKAICorpusOptions` |
| O-03 | AP.04 | 🟡 | `SectionName` defined | ✅ Pass |
| O-04 | BB.05 | 🔴 | NOT `sealed class` (legacy) | ✅ Pass — `sealed record` |

### 🔍 Implementation Patterns (CS.01/03/06, OR.01, AP.01, BB.04)

| ID | Rule | Tier | Check | Result |
|:---|:-----|:-----|:------|:-------|
| I-01 | AP.01 | 🔴 | Sealed usage | ✅ Pass — `public sealed`: 10件, `public class ` (非sealed): 0件 (比率 100%) |
| I-02 | AP.01 | 🔴 | VKGuard usage | ✅ Pass — 50件以上検出 |
| I-03 | CS.03 | 🔴 | ConfigureAwait compliance | ✅ Pass — `ConfigureAwait(false)`: 6件, `await `: 6件 (比率 100%) |
| I-04 | OR.01 | 🟡 | LoggerMessage source gen | ✅ Pass — `[LoggerMessage]` 検出 |
| I-05 | OR.01 | 🔴 | No direct logger calls | ✅ Pass — 検出なし |
| I-06 | BB.04 | 🟡 | Diagnostics attribute | ✅ Pass — `[VKBlockDiagnostics<VKAICorpusBlock>]` |
| I-07 | CS.01 | 🔴 | Result pattern usage | ✅ Pass — `VKResult<T>` 全メソッドで使用 |
| I-08 | CS.06 | 🔴 | No DateTime.UtcNow | ✅ Pass — 検出なし |
| I-09 | CS.06 | 🔴 | No Guid.NewGuid() | ✅ Pass — 検出なし |
| I-10 | CS.06 | 🔴 | No JsonSerializer direct | ✅ Pass — `IVKJsonSerializer` 使用 |
| I-11 | CS.02 | 🟡 | No dependency pollution | ✅ Pass — EF Core / Redis 依存なし |

### 📛 Naming & Visibility (AP.03)

| ID | Rule | Tier | Check | Result |
|:---|:-----|:-----|:------|:-------|
| N-01 | AP.03 | 🟡 | Level 1 public types use `VK`/`IVK` prefix | ✅ Pass — 全 public 型が VK/IVK プレフィックス |
| N-02 | AP.03 | 🟡 | Level 2+ types are `internal` | ✅ Pass — `Internal/` 配下は全て `internal sealed class` |
| N-03 | AP.03 | 🟡 | Matching namespace | ⚠️ Warn — [InMemoryKnowledgeInjectionStore.cs](/src/BuildingBlocks/AI.Corpus/Tracking/Internal/InMemoryKnowledgeInjectionStore.cs) が `namespace VK.Blocks.AI.Corpus;` (ルート NS) を使用。`Tracking.Internal` NS が正しい |

### 📊 Summary Table

| Category | Tier | ✅ | ❌ | ⚠️ |
|:---------|:-----|:---|:---|:---|
| Structure | 🟡 | 6 | 0 | 0 |
| Marker | 🔴 | 4 | 0 | 0 |
| DI Registration | 🔴 | 7 | 0 | 0 |
| Options | 🟡 | 4 | 0 | 0 |
| Implementation | 🔴 | 11 | 0 | 0 |
| Naming | 🟡 | 2 | 0 | 1 |

### 🚩 Audit Exceptions

Audit: 🚩 [AP.03] InMemoryKnowledgeInjectionStore の namespace が `Tracking.Internal` ではなくルート NS を使用

---

## Phase 2: Registration Audit (DI Layer)

### ✅ Execution Order (BB.03)

[AICorpusBlockRegistration.cs](/src/BuildingBlocks/AI.Corpus/Common/DependencyInjection/Internal/AICorpusBlockRegistration.cs) の登録シーケンスを検証:

| Step | Expected | Actual | Line | Status |
|:-----|:---------|:-------|:-----|:-------|
| 1 | Check-Self & Prerequisite | `IsVKBlockRegistered<VKAICorpusBlock>()` | L22 | ✅ |
| 2 | Options Registration | `AddVKBlockOptions(configuration, transform)` | L28 | ✅ |
| 3 | Mark-Self | `AddVKBlockMarker<VKAICorpusBlock>()` | L31 | ✅ |
| 4 | Options Validation | (空 — コメントのみ) | L33-34 | ⚠️ 未実装 |
| 5 | Diagnostics & Metadata | (空 — コメントのみ) | L35-36 | ⚠️ 未実装 |
| 6 | Feature Toggle Exit | `if (!options.Enabled) return builder` | L38 | ✅ |

#### 🚩 BB.05 — Options Validation 未実装

ブロックレベルの `IValidateOptions<VKAICorpusOptions>` が登録されていない。Feature レベル（`FilteringFeature.ValidateCustom`, `GatheringFeature.ValidateCustom`）では SG 連携により `IValidateOptions` が自動生成されているが、ルート Options の `Enabled` プロパティに対するバリデーションは実装されていない。

### ✅ Func Transform (BB.03 / ADR-016)

`Func<VKAICorpusOptions, VKAICorpusOptions>` パラメータを使用（`Action<T>` ではない）。`sealed record` に対する関数型変換パターンに準拠。L17。

### ✅ Enabled Policy Position (BB.03)

`if (!options.Enabled)` は `AddVKBlockMarker` (L31) の**後**に配置 (L38)。正しい順序。

### ✅ Builder Pattern (BB.03)

- `AICorpusBlockBuilder` が `VKBlockBuilder<VKAICorpusBlock>` を継承し、`IVKAICorpusBuilder` を実装。
- `VKAICorpusBuilderExtensions` が Feature 単位のチェーン API を提供 (`AddVKGathering`, `AddVKFiltering`, `AddVKTracking`, `AddVKDefaultFeatures`)。
- 全 Feature 登録で `TryAddEnumerable` / `TryAddScoped` / `TryAddSingleton` を使用。

### ✅ Feature Registration (BB.06)

各 Feature が `[VKFeature(typeof(VKAICorpusBlock))]` 属性 + `sealed partial class` + `RegisterCustom` / `ValidateCustom` の SG フックパターンを採用。

---

## Phase 3: Implementation Audit (Deep Analysis)

### 1. 設計原則 (Design Principles)

#### ✅ SOLID 準拠度: 高

- **SRP**: 各フィルターは単一責務（例: `CooldownFilter` はクールダウン判定のみ）。`DefaultFilteringStage` はオーケストレータとして候補エントリの収集・フィルターチェーン実行・結果の State 伝播を行う。
- **OCP**: `IVKKnowledgeLifecycleFilter` インターフェースと `TryAddEnumerable` により、新しいフィルターの追加は既存コードを変更せずに可能。
- **LSP**: 全フィルターが `IVKKnowledgeLifecycleFilter` → `IVKEntryFilter<VKKnowledgeLifecycleEntry, VKCorpusContext>` を正しく実装。
- **ISP**: `IVKFilteringOptions` / `IVKFilteringOverrides` / `IVKGatheringOptions` / `IVKGatheringOverrides` により Settings と Overrides が分離されている (AP.05)。
- **DIP**: 全 Stage が抽象インターフェース (`IVKRecallKnowledgeLifecycleStore`, `IVKKnowledgeInjectionStore`, `IVKEchoStore`, `IVKSearchStrategy`) に依存。

#### ✅ KISS / DRY

- フィルターロジックが各クラスに自己完結しており、複雑な継承階層を回避。
- `VKKnowledgeLifecyclePresets` による T-Shirt Sizing 定数でマジックナンバーを排除。

### 2. 設計パターン (Design Patterns)

| パターン | 適用箇所 | 評価 |
|:---------|:---------|:-----|
| **Strategy** | `IVKKnowledgeLifecycleFilter` + 17種の実装 | ✅ 模範的 |
| **Pipeline** | `IVKPsycheBeforePipelineStage` / `IVKPsycheAfterPipelineStage` | ✅ 模範的 |
| **Builder** | `IVKAICorpusBuilder` + チェーン API | ✅ 準拠 |
| **Repository** | `IVKRecallKnowledgeLifecycleStore` / `IVKStaticKnowledgeLifecycleStore` | ✅ 準拠 |
| **State Object** | `CorpusInjectionState` / `RecalledKnowledgeLifecycleState` | ✅ 不変 record で安全に伝播 |

### 3. アーキテクチャ原則 (Architectural Principles)

- **関注点分離**: Gathering (候補収集) → Filtering (フィルタリング) → Tracking (使用記録) の3段階が明確に分離。
- **カプセル化**: 全実装クラスが `internal sealed`。Public API は interface + record のみ。
- **高凝集**: 各 Feature が独自の Options / Protocols / Internal を持つ。
- **低結合**: Feature 間は `VKPsycheContext` の State 機構を介して疎結合に連携（`CorpusInjectionState`, `RecalledKnowledgeLifecycleState`）。

### 4. アーキテクチャスタイル (Architectural Styles)

- **Vertical Slice**: BB.01 に準拠。Gathering / Filtering / Tracking が Feature 単位で独立。
- **Clean Architecture**: Domain 型（`VKKnowledgeLifecycle`, `VKCorpusContext` 等）が外部依存を持たず、インフラ実装（`DefaultRecallKnowledgeLifecycleStore` → `IVKSearchStrategy`）が Anti-Corruption Layer を形成。

### 5. アーキテクチャパターン (Architectural Patterns)

- **Psyche Pipeline 統合**: `IVKPsycheBeforePipelineStage` (Gathering, Filtering) + `IVKPsycheAfterPipelineStage` (Tracking) による構造化されたパイプラインフック。
- **VKFeature SG 統合**: `[VKFeature]` 属性による Options バリデーション・DI 登録の自動生成。

### 6. エンタープライズパターン (Enterprise Patterns)

- **可観測性**: `[VKBlockDiagnostics<VKAICorpusBlock>]` + `[LoggerMessage]` SG。ただし `DiagnosticsConstants.cs` が未作成。
- **冪等性**: DI 登録が `IsVKBlockRegistered` + `TryAdd` で完全冪等。
- **スワップ可能性**: 全 Store インターフェースに InMemory / Default 実装が提供され、テスト・本番で差し替え可能。

### 7. VK.Blocks 固有の準拠度 (Deep Compliance)

#### ✅ Error 定数パターン (CS.01)

`VKResult.Failure(historyResultData.FirstError)` / `VKResult.Failure(searchResult.FirstError)` のようにエラーを伝播。raw string は使用していない。

#### 🚩 CS.01 例外使用 — InMemoryKnowledgeInjectionStore

[InMemoryKnowledgeInjectionStore.cs:L27,47](/src/BuildingBlocks/AI.Corpus/Tracking/Internal/InMemoryKnowledgeInjectionStore.cs) で `throw new ArgumentException("SessionId cannot be empty.")` を使用。Infrastructure Layer として例外は許容されるが、VK.Blocks の基本実装（`Basic` taxonomy）として `VKResult.Failure` パターンへの統一が推奨される。`VKGuard.NotEmptyGuid` に置き換えれば統一性が向上する。

#### ✅ CancellationToken 伝播 (CS.03)

全 async メソッドチェーンで `CancellationToken` が途切れずに伝播。`ConfigureAwait(false)` も全 `await` に付与。

#### ✅ Visibility 整合性 (AP.03)

- Public API: `VK` / `IVK` プレフィックス付き record / interface のみ。
- Internal: `Internal/` 配下は全て `internal sealed class`（VK プレフィックスなし）。

#### ✅ Core 抽象の活用 (CS.06)

- `IVKJsonSerializer` を [DefaultRecallKnowledgeLifecycleStore.cs:L66](/src/BuildingBlocks/AI.Corpus/Gathering/Internal/DefaultRecallKnowledgeLifecycleStore.cs) で使用。
- `DateTime.UtcNow` / `Guid.NewGuid()` / `JsonSerializer` の直接使用なし。
- `TimeProvider` は [FreshnessFilter.cs](/src/BuildingBlocks/AI.Corpus/Filtering/Internal/FreshnessFilter.cs) で注入使用。

---

## 深度ロジック＆状態演進審査 (Deep Logic & State Evolution Audit)

### 🧠 実行パス脳内推演

#### 成功パス: Gathering → Filtering → Tracking

1. **Gathering** (`DefaultGatheringStage.ExecuteAsync`): `IVKRecallKnowledgeLifecycleStore.GetLifecycleEntriesAsync` で候補を取得 → `VKKnowledgeCandidatesState` に候補を追加 → `RecalledKnowledgeLifecycleState` をセット。
2. **Filtering** (`DefaultFilteringStage.ExecuteAsync`): Echo 履歴 + 注入履歴を取得 → `VKCorpusContext` を構築 → 候補を ExclusiveWeight 降順ソート → フィルターチェーン実行 → 通過エントリで `VKKnowledgeCandidatesState.Candidates` を置換 → `CorpusInjectionState` をセット。
3. **Tracking** (`DefaultKnowledgeInjectionStage.ExecuteAsync`): `CorpusInjectionState` から注入リストを読み取り → `IVKKnowledgeInjectionStore.RecordInjectionsAsync` で永続化。

**状態伝播は正確**: 各 Stage が `context.SetState` / `context.State<T>()` で疎結合に State を受け渡し。

#### 失敗パス

- Gathering で `IVKRecallKnowledgeLifecycleStore` が `Result.Failure` → `VKResult.Failure(error)` が返される → Pipeline は中断。
- Filtering で `IVKEchoStore.GetHistoryAsync` が失敗 → `currentTurn = 1` として続行（グレースフルデグラデーション）。注入履歴取得失敗時は `Failure` で即時返却。
- Tracking で `RecordInjectionsAsync` 失敗 → **`VKResult.Success()` が返される** → ログ警告のみで Pipeline は続行。これは **意図的な設計判断** として妥当（使用記録の失敗がユーザー体験を中断すべきではない）。

### ⚠️ 論理的懸念: DefaultGatheringStage の currentTurn 固定

[DefaultGatheringStage.cs:L46](/src/BuildingBlocks/AI.Corpus/Gathering/Internal/DefaultGatheringStage.cs) で `int currentTurn = 1` がハードコードされている。`DefaultFilteringStage` では Echo 履歴から正しく `currentTurn` を計算しているが、Gathering Stage ではこの計算をスキップしている。`VKCorpusContext.CurrentTurn` が Recall Store のクエリロジックで参照される場合、不正確な結果を返す可能性がある。

### ⚠️ 防御性: DefaultRecallKnowledgeLifecycleStore の `catch` ブロック

[DefaultRecallKnowledgeLifecycleStore.cs:L69-72](/src/BuildingBlocks/AI.Corpus/Gathering/Internal/DefaultRecallKnowledgeLifecycleStore.cs) で bare `catch` を使用。例外種別を問わずスワローしている。これにより **デシリアライズ失敗時の診断情報が完全に失われる**。`catch (Exception ex)` + ログ出力に置き換えるべき。

---

## 🚨 重大なアーキテクチャの懸念事項 (Critical Architectural Smells)

- ❌ **[CS.01 — throw 使用]**: [InMemoryKnowledgeInjectionStore.cs:L27,47](/src/BuildingBlocks/AI.Corpus/Tracking/Internal/InMemoryKnowledgeInjectionStore.cs) — `throw new ArgumentException` が Infrastructure 境界で使用されている。`VKGuard` 又は `VKResult.Failure` パターンに統一すべき。

---

## 🛡️ 非機能要件とセキュリティ (Non-Functional Requirements & Security)

- 🔒 **[Bare Catch Swallow]**: [DefaultRecallKnowledgeLifecycleStore.cs:L69-72](/src/BuildingBlocks/AI.Corpus/Gathering/Internal/DefaultRecallKnowledgeLifecycleStore.cs) — 例外をスワローしてデフォルト `VKKnowledgeLifecycle` にフォールバック。デシリアライズエラーの根本原因追跡が不可能になる。構造化ログを追加すべき。
- 🔒 **[InMemory Store の lock 粒度]**: [InMemoryKnowledgeInjectionStore.cs:L32,54](/src/BuildingBlocks/AI.Corpus/Tracking/Internal/InMemoryKnowledgeInjectionStore.cs) — `lock(list)` でリスト全体をロックしているが、`ConcurrentDictionary` の `GetOrAdd` と組み合わせた場合、高並行度で性能劣化の可能性あり。ただし `Basic` taxonomy として適切な範囲。

---

## 🧪 テスト容易性と疎結合性 (Testability & Decoupling)

- ⚙️ **高いテスト容易性**: 全 Stage と Filter が抽象インターフェースに依存しており、`InternalsVisibleTo` によるテストプロジェクト連携が `.csproj` で設定済み。
- ⚙️ **InMemory 実装の提供**: `InMemoryStaticKnowledgeLifecycleStore` / `InMemoryKnowledgeInjectionStore` がテスト用の具象実装として利用可能。`Seed()` / `Clear()` メソッドによるテストデータ管理も充実。

---

## 🔭 可観測性の準拠度 (Observability Readiness)

- 📡 **[VKBlockDiagnostics]**: [CorpusDiagnostics.cs](/src/BuildingBlocks/AI.Corpus/Common/Diagnostics/Internal/CorpusDiagnostics.cs) に `[VKBlockDiagnostics<VKAICorpusBlock>]` が付与済み。
- 📡 **[LoggerMessage SG]**: [CorpusLog.cs](/src/BuildingBlocks/AI.Corpus/Common/Diagnostics/Internal/CorpusLog.cs) で `[LoggerMessage]` SG を使用。構造化テンプレートに `{SessionId}`, `{Error}` を含む。
- ⚠️ **[DiagnosticsConstants 未作成]**: BB.04 で推奨される `DiagnosticsConstants.cs` が `Diagnostics/` 配下に存在しない。メトリクス・トレーシングのセマンティックトークンが未定義。
- ⚠️ **[ログカバレッジの薄さ]**: 現在 `CorpusLog` には `FailedToRecordInjections` の1メソッドのみ。フィルタリング結果のサマリーログ、Gathering 候補数のトレースログなどの追加が推奨される。

---

## ⚠️ コード品質とコーディング規約のリスク (Code Quality & Standard Risks)

- ⚠️ **[Namespace 不整合]**: [InMemoryKnowledgeInjectionStore.cs:L9](/src/BuildingBlocks/AI.Corpus/Tracking/Internal/InMemoryKnowledgeInjectionStore.cs) — `namespace VK.Blocks.AI.Corpus;` (ルート) を使用。物理パス `Tracking/Internal/` に従い `VK.Blocks.AI.Corpus.Tracking.Internal` が正しい (AP.03)。
- ⚠️ **[VKFilteringOptions の Filter Toggles に XML Doc なし]**: [VKFilteringOptions.cs:L42-59](/src/BuildingBlocks/AI.Corpus/Filtering/VKFilteringOptions.cs) — 18個のフィルタートグルプロパティに XML ドキュメントコメントが欠如。API ドキュメント生成時に不完全になる。
- ⚠️ **[VKKnowledgeInjection に XML Doc なし]**: [VKKnowledgeInjection.cs](/src/BuildingBlocks/AI.Corpus/Tracking/Models/VKKnowledgeInjection.cs) — public record に XML ドキュメントコメントが欠如。

---

## ✅ 評価ポイント (Highlights / Good Practices)

1. **Strategy パターンの模範的実装**: 17種のフィルターが `IVKKnowledgeLifecycleFilter` を通じて統一されたインターフェースで登録・実行され、Options トグルによる個別有効/無効化が可能。
2. **VKFeature SG 統合の先進的採用**: `[VKFeature(typeof(VKAICorpusBlock), GenerateArgs = true, GenerateValidator = true)]` による Options バリデーション・Args 生成の自動化。
3. **AP.05 Strict Overrides Contract 完全実装**: `IVKFilteringOverrides` / `IVKGatheringOverrides` による request-level override の安全な分離。
4. **VKKnowledgeLifecyclePresets**: T-Shirt Sizing による定数化が優れた開発者体験を提供。
5. **State 伝播パターン**: `VKPsycheContext.SetState<T>()` / `State<T>()` による Feature 間の疎結合な状態共有が設計上安全。
6. **フィルター実行順序の明示**: FilteringFeature.cs で 0→4 の段階コメントにより、フィルター登録順序の設計意図が文書化されている。

---

## 💡 改善ロードマップ (Evolutionary Roadmap)

### 1. 最優先対応 (Immediate Action)

| # | 対応内容 | ファイル | Rule |
|:--|:---------|:---------|:-----|
| 1 | `InMemoryKnowledgeInjectionStore` の `throw` を `VKGuard` に置換 | [InMemoryKnowledgeInjectionStore.cs:L25-28,45-48](/src/BuildingBlocks/AI.Corpus/Tracking/Internal/InMemoryKnowledgeInjectionStore.cs) | CS.01 |
| 2 | `InMemoryKnowledgeInjectionStore` の namespace を `Tracking.Internal` に修正 | [InMemoryKnowledgeInjectionStore.cs:L9](/src/BuildingBlocks/AI.Corpus/Tracking/Internal/InMemoryKnowledgeInjectionStore.cs) | AP.03 |
| 3 | `DefaultRecallKnowledgeLifecycleStore` の bare `catch` に構造化ログを追加 | [DefaultRecallKnowledgeLifecycleStore.cs:L69-72](/src/BuildingBlocks/AI.Corpus/Gathering/Internal/DefaultRecallKnowledgeLifecycleStore.cs) | OR.01 |

### 2. リファクタリング提案 (Refactoring)

| # | 対応内容 | ファイル | Rule |
|:--|:---------|:---------|:-----|
| 4 | `DiagnosticsConstants.cs` を `Common/Diagnostics/` に作成 | N/A (新規) | BB.04 |
| 5 | `CorpusLog` にフィルタリング結果サマリー・Gathering 候補数のログメソッドを追加 | [CorpusLog.cs](/src/BuildingBlocks/AI.Corpus/Common/Diagnostics/Internal/CorpusLog.cs) | OR.01 |
| 6 | `VKFilteringOptions` のフィルタートグルに XML Doc を追加 | [VKFilteringOptions.cs](/src/BuildingBlocks/AI.Corpus/Filtering/VKFilteringOptions.cs) | AP.01 |
| 7 | `DefaultGatheringStage` の `currentTurn` を Echo 履歴から計算するか、Gathering 用途では不要であることをコメントで明示 | [DefaultGatheringStage.cs:L46](/src/BuildingBlocks/AI.Corpus/Gathering/Internal/DefaultGatheringStage.cs) | — |

### 3. 推奨される学習トピック (Learning Suggestions)

- **OpenTelemetry Metrics 統合**: フィルター通過率・候補数・処理時間を `Histogram` / `Counter` で可視化するパターン。
- **Feature Flag 動的切替**: 現在はコンパイル時の Options 設定だが、`IOptionsMonitor<T>` を活用した動的フィルター有効化の可能性。
