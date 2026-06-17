# 🏗️ アーキテクチャ監査レポート — AI.Psyche

**対象モジュール**: `VK.Blocks.AI.Psyche`
**監査日**: 2026-06-17
**監査バージョン**: Full Architecture Audit (Phase 1–4)
**Audit**: 🚩 [CS.01] InMemory Stores で `throw new` パターンが残存

---

## 📊 監査サマリー (Audit Summary)

- **総合スコア**: **88 / 100**
- **Fast Audit スコア**: 17/18 (94%) — `throw new` 検出により CS.01 で 1 件 Fail
- **対象レイヤー判定**: Application Layer / Prompt Orchestration & Behaviors Pipeline (BuildingBlock)
- **総評 (Executive Summary)**:
  AI.Psyche は前回監査（2026-05-31: 91点）から Behaviors / Middleware / Pipeline Executor を含む大幅な機能拡張を経て、より完成度の高いプロンプトオーケストレーションフレームワークに進化した。Before / Middleware / After の 3 フェーズパイプライン、Onion ミドルウェアチェーン、Pattern Feature（Few-Shot パターン注入）の追加により、機能的成熟度は大きく向上している。`VKGuard` 境界防御、`ConfigureAwait(false)` の徹底、`Result<T>` パターン、`[LoggerMessage]` SG ロギング、`Func<T,T>` Transform パターンなど、Industrial DNA の主要要件への準拠は引き続き高水準を維持。ただし、前回指摘の「InMemory Store の `throw new` パターン」が **6 ファイル・11 箇所で依然残存** しており、CS.01 違反が改善されていない点が最大の減点要因。また `DefaultPsychePipelineExecutor.InvokeChatEngineAsync` にインライン `new VKError(...)` が 1 件残存。前回指摘の `FormatterNotFound` Description `"demo"` 問題は修正済み。前回指摘の `_evicted` スレッドセーフ性問題は `VKPsycheEvictedState` への置き換えにより解消。

---

## 🚨 重大なアーキテクチャの懸念事項 (Critical Architectural Smells)

_該当なし_ — レイヤー間の依存逆転、循環依存、致命的パフォーマンスボトルネックは検出されなかった。

---

## 🛡️ 非機能要件とセキュリティ (Non-Functional Requirements & Security)

- 🔒 **[CS.01 違反 — InMemory Stores の `throw new` パターン]**:
  全 5 つの InMemory Store 実装が入力バリデーションに `throw new ArgumentException` を使用しており、VK.Blocks の `Result<T>` / `VKGuard` パターンから逸脱。対象ファイルと箇所数：

  | ファイル | 箇所数 |
  |:---------|:------:|
  | [InMemoryEchoStore.cs](/src/BuildingBlocks/AI.Psyche/Echo/Internal/InMemoryEchoStore.cs) | 4 |
  | [InMemoryDirectiveStore.cs](/src/BuildingBlocks/AI.Psyche/Directive/Internal/InMemoryDirectiveStore.cs) | 2 |
  | [InMemoryKnowledgeStore.cs](/src/BuildingBlocks/AI.Psyche/Knowledge/Internal/InMemoryKnowledgeStore.cs) | 1 |
  | [InMemoryPersonaStore.cs](/src/BuildingBlocks/AI.Psyche/Persona/Internal/InMemoryPersonaStore.cs) | 2 |
  | [InMemoryPatternStore.cs](/src/BuildingBlocks/AI.Psyche/Pattern/Internal/InMemoryPatternStore.cs) | 1 |
  | [PromptPositionResolver.cs](/src/BuildingBlocks/AI.Psyche/Common/Internal/PromptPositionResolver.cs) | 1 |

  **推奨対応**: `throw new ArgumentException` → `VKGuard.NotEmptyGuid(id)` または `Result.Failure(VKXxxErrors.InvalidId)` に置換。`PromptPositionResolver` の `ArgumentOutOfRangeException` は内部 switch 式のため許容度は高いが、`Result.Failure` パターンへの移行が望ましい。

- 🔒 **[CS.01 違反 — インライン VKError]**: [DefaultPsychePipelineExecutor.cs:L166](/src/BuildingBlocks/AI.Psyche/Behaviors/Pipeline/Internal/DefaultPsychePipelineExecutor.cs) で `new VKError("AI.Psyche.ChatEngineNotFound", ...)` をインラインで生成。`VKBehaviorsErrors` に `static readonly` 定数として集約すべき。

- 🔒 **[CS.07 — GetService パターン]**: [DefaultPsychePipelineExecutor.cs:L164](/src/BuildingBlocks/AI.Psyche/Behaviors/Pipeline/Internal/DefaultPsychePipelineExecutor.cs) で `ctx.Services.GetService(typeof(IVKChatEngine))` を使用。Service Locator パターンだが、`IVKChatEngine` は WeaveOnly モード時に不要なため null 許容が設計上妥当。`is not IVKChatEngine chatEngine` ガードによるフォールバックあり。CS.07 準拠（ドキュメント化されたフォールバック）。

- 🔒 **[スレッドセーフ性 — VKPsycheContext._states]**: `Dictionary<Type, object> _states` は `Lock` 保護なしで `SetState` / `State` が呼ばれる。現在の実行フローでは Before ステージ間で並列書き込みが発生しうるが、異なる型キーへの書き込みが主であるため実用上の問題は低い。将来的に `ConcurrentDictionary` への変更を推奨。

- 🔒 **[DefaultPromptTruncateTask — TimeProvider optional DI]**: [DefaultPromptTruncateTask.cs:L29](/src/BuildingBlocks/AI.Psyche/Weaving/Internal/DefaultPromptTruncateTask.cs) で `TimeProvider? timeProvider = null` と nullable optional パラメータ。前回指摘の `DefaultTapestryWeavingTask` の similar issue は修正済みだが、TruncateTask に同パターンが存在。ただし `TimeProvider` は現在未使用フィールドでもある。

---

## 🧪 テスト容易性と疎結合性 (Testability & Decoupling)

- ⚙️ **[優れた疎結合設計]**: 全ステージ・ミドルウェアがインターフェース経由で DI 登録（`IVKPsycheBeforePipelineStage`, `IVKPsycheAfterPipelineStage`, `IVKPsycheMiddleware`, `IVKWeavingTask`）。`TryAddEnumerable` によるマルチ実装パターンを全面採用。テスト時のモック差し替えが容易。
- ⚙️ **[InternalsVisibleTo]**: `.csproj` で `VK.Blocks.AI.Psyche.UnitTests`, `VK.Blocks.AI.Engram`, `VK.Blocks.AI.Engram.UnitTests`, `DynamicProxyGenAssembly2` への公開が設定済み。Moq による internal クラスのモック化も可能。
- ⚙️ **[Middleware パターン]**: Onion-style のミドルウェアチェーンにより、LLM 呼び出しの前後にカスタムロジック（レート制限、キャッシュ、監査等）を注入可能。高いテスタビリティ。
- ⚙️ **[PsychePipelineRunner]**: `VKWeavingStepRunner` と並列で `PsychePipelineRunner` が Behaviors 層にも追加され、Before/After ステージの並列実行を管理。ジェネリック設計で再利用可能。

---

## 🔭 可観測性の準拠度 (Observability Readiness)

- 📡 **[OR.01 完全準拠]**: 全 6 Feature の Diagnostics クラスが `[LoggerMessage]` Source Generator パターンを使用。`logger.LogXxx()` 直呼び出しは **ゼロ件**。
- 📡 **[VKBlockDiagnostics 属性]**: 全 Feature の Internal Diagnostics クラスに `[VKBlockDiagnostics<VKAIPsycheBlock>]` が付与。BB.04 準拠。
- 📡 **[DiagnosticConstants パターン]**: 各 Feature に公開 Diagnostics 定数クラスが存在：
  - `VKBehaviorsDiagnostics` (EventId + Metrics)
  - `VKEchoDiagnostics` (EventId)
  - `VKWeavingDiagnostics` (EventId)
  - `VKKnowledgeDiagnosticTokens` (EventId)
  - `VKDirectiveDiagnostic` (EventId)
- 📡 **[CorrelationId 伝播]**: `DefaultPsychePipeline` が `IVKGuidGenerator` でトレース ID を自動生成し、全ログメッセージに伝播。
- 📡 **[Metrics 定数未計測]**: `VKBehaviorsDiagnostics.Metrics.PipelineDuration` / `.ExecutionDuration` が定義されているが、実際の `Histogram<double>` 計測は未実装。

---

## ⚠️ コード品質とコーディング規約のリスク (Code Quality & Standard Risks)

### Phase 1 (Fast Audit) — 構造チェック

| # | ルール | ティア | チェック | 結果 |
|:--|:-------|:-------|:---------|:-----|
| S-01 | BB.01 | 🔴 | Root に `VK{Module}Block.cs` のみ | ✅ Pass — `VKAIPsycheBlock.cs` + `VKAIPsycheOptions.cs` (Options は root 許容) |
| S-02 | BB.01 | 🔴 | `Common/` ディレクトリ存在 | ✅ Pass |
| S-03 | BB.04 | 🟡 | `Diagnostics/` 存在 | ✅ Pass — 各 Feature に `Diagnostics/` |
| S-04 | BB.04 | 🟡 | `Diagnostics/Internal/` 存在 | ✅ Pass — 全 Feature に配置 |
| S-05 | BB.01 | 🟡 | `Common/DependencyInjection/` 存在 | ✅ Pass |
| S-06 | BB.01 | 🟡 | `Common/DependencyInjection/Internal/` 存在 | ✅ Pass |
| S-07 | BB.02 | 🔴 | `[VKBlockMarker]` on `sealed partial class` | ✅ Pass |
| S-08 | BB.02 | 🔴 | Dependencies 宣言 | ✅ Pass — `[typeof(VKAIBlock)]` |
| S-09 | BB.01 | 🔴 | Feature フォルダがドメイン名詞 | ✅ Pass — Behaviors, Directive, Echo, Knowledge, Pattern, Persona, Weaving |
| S-10 | AP.03 | 🟡 | Internal/ 内のクラスが `internal` | ✅ Pass |
| S-11 | AP.03 | 🟡 | Internal/ 内のクラスが VK prefix なし | ⚠️ Warn — `VKWeavingStepRunner` (Common/Models 配置, internal, VK prefix) |
| S-12 | AP.01 | 🔴 | 全クラスが `sealed` | ✅ Pass |
| S-13 | CS.06 | 🔴 | `DateTime.UtcNow` / `Guid.NewGuid()` ゼロ | ✅ Pass |
| S-14 | OR.01 | 🔴 | `logger.LogXxx()` 直呼びゼロ | ✅ Pass |
| S-15 | CS.01 | 🔴 | `throw new` ゼロ | ❌ Fail — 11 箇所 (InMemory Stores 10 + PromptPositionResolver 1) |
| S-16 | AP.01 | 🔴 | `default!` ゼロ | ✅ Pass |
| S-17 | CS.03 | 🔴 | 全 `await` に `.ConfigureAwait(false)` | ✅ Pass — 17 箇所全て付与 |
| S-18 | BB.01 | 🟡 | Options が独自ファイルに分離 (BB.07) | ✅ Pass — 全 Options が独自 `.cs` ファイル |

**Fast Audit スコア**: 17/18 (94%)

### Phase 2 (DI Registration Audit)

#### BB.03 — Block Level Registration

[AIPsycheBlockRegistration.cs](/src/BuildingBlocks/AI.Psyche/Common/DependencyInjection/Internal/AIPsycheBlockRegistration.cs) の 8 ステップ検証：

| ステップ | 期待 | 実装 | 判定 |
|:---------|:-----|:-----|:-----:|
| 1. Check-Self | `IsVKBlockRegistered<T>()` | ✅ L20 | PASS |
| 2. Options | `AddVKBlockOptions(...)` | ✅ L24 | PASS |
| 3. Mark-Self | `AddVKBlockMarker<T>()` | ✅ L26 | PASS |
| 4. Validator | SG 自動生成 | ✅ `[VKFeature]` 属性付き | PASS |
| 5. Diagnostics | SG 自動生成 | ✅ `[VKBlockDiagnostics]` 存在 | PASS |
| 6. Toggle | `if (!options.Enabled)` AFTER Mark | ✅ L28 | PASS |
| 7. Core Services | Feature 別登録 | ✅ Builder 経由 | PASS |

#### BB.03 — Func Transform

`Func<VKAIPsycheOptions, VKAIPsycheOptions>` パターンを使用（`Action<T>` ではない）。✅ ADR-016 準拠。
全 Feature Extension も `Func<TOptions, TOptions>? transform` パターンを使用。✅

#### BB.03 — Enabled Policy Position

`if (!options.Enabled)` が `AddVKBlockMarker` (L26) の**後** (L28) に配置。✅ PASS。

#### BB.05 — OptionsValidator Quality

全 Options が `sealed partial record` + `[VKFeature]` SG 連携。`EchoFeature.ValidateCustom` で `TokenBudgetRatio`, `MaxWindowSize`, `MaxTokens`, `MaxTurns` の境界値検証。`KnowledgeFeature.ValidateCustom` で `MaxEntriesToInject`, `ReservedTokens`, `SemanticThreshold` の境界値検証。✅ PASS。

### Phase 3 (Implementation Audit) — VK.Blocks 固有準拠度

| # | ルール | ティア | チェック | 結果 |
|:--|:-------|:-------|:---------|:-----|
| I-01 | CS.01 | 🔴 | Error 定数パターン | ⚠️ Warn — 1 件 inline `new VKError(...)` (PipelineExecutor L166) |
| I-02 | CS.03 | 🔴 | CancellationToken 伝播 | ✅ Pass — 全 async チェーンで途切れなし |
| I-03 | CS.03 | 🔴 | ConfigureAwait(false) | ✅ Pass — 全 17 箇所 |
| I-04 | AP.01 | 🔴 | VKGuard 境界防御 | ✅ Pass — 全 public/internal メソッドエントリポイント |
| I-05 | CS.06 | 🔴 | Core 抽象活用 | ✅ Pass — `IVKGuidGenerator` 使用、`DateTime.UtcNow` ゼロ |
| I-06 | BB.04 | 🟡 | Diagnostics 属性 | ✅ Pass — 全 Feature に `[VKBlockDiagnostics<VKAIPsycheBlock>]` |
| I-07 | AP.03 | 🟡 | Visibility 整合性 | ⚠️ Warn — `VKWeavingStepRunner` (internal + VK prefix) |
| I-08 | BB.06 | 🟡 | Feature Pattern | ✅ Pass — 全 7 Feature が `[VKFeature]` SG + chained builder |
| I-09 | DL.05 | 🟡 | SG Hook タグ | ✅ Pass — 全 Feature に `// [SG Hook]` 付き partial methods |

---

## ✅ 評価ポイント (Highlights / Good Practices)

### 設計原則 (Design Principles)

- **SRP 徹底**: 7 Feature（Behaviors, Directive, Echo, Knowledge, Pattern, Persona, Weaving）が独立したフォルダに分離され、単一責任の境界が明確。前回の 6 Feature から Pattern + Behaviors が追加。
- **DIP 完全準拠**: 全依存関係がインターフェース経由。具象クラスへの直接依存ゼロ。
- **Result<T> パターン**: パイプライン・ステージ間の全エラーフローが `VKResult<T>` で統一。

### 設計パターン (Design Patterns)

- **Strategy パターン**: `IVKPromptFormatter` による Feature 別フォーマット戦略。`CanFormat` ガードによる動的ディスパッチ。
- **Pipeline / Chain of Responsibility**: Before → Middleware → After の 3 フェーズパイプライン。
- **Onion Middleware**: `DefaultPsychePipelineExecutor` が reverse-order wrap で Middleware チェーンを構築。ASP.NET Core スタイルの拡張ポイント。
- **Template Method (via Generics)**: `VKWeavingStepRunner.ExecuteChunksAsync<T>` + `PsychePipelineRunner.ExecuteChunksAsync<T>` によるジェネリック並列実行。
- **Builder (Fluent)**: `IVKAIPsycheBuilder` + `AddVKDefaultFeatures()` による宣言的 Feature 有効化。

### アーキテクチャ原則

- **関心点分離**: 抽出（Before Stages） → ミドルウェア（LLM 呼び出し制御） → 後処理（After Stages）の 3 段階が明確に分離。
- **高凝集・低結合**: Knowledge の `VKKnowledgeMatcher` は Expression Tree コンパイルをカプセル化した自己完結型。
- **スレッドセーフ設計**: `VKPsycheContext._fragments` の `Lock` 保護、`Interlocked` による Abort フラグ管理。

### VK.Blocks 固有準拠度

- **CS.06**: `IVKGuidGenerator` 使用（`DefaultPsychePipeline` L41）。非確定的 API 直呼びゼロ。
- **CS.03**: 全 `await` に `.ConfigureAwait(false)` 付与。`CancellationToken` 全チェーン伝播。
- **AP.01**: 全クラスが `sealed`。全 Options が `sealed record`。`required` キーワード使用。`VKGuard` 境界防御徹底。
- **BB.02**: `[VKBlockMarker(Dependencies = [typeof(VKAIBlock)])]` 正しく宣言。
- **BB.06**: 7 Feature が `[VKFeature]` SG + Chained Builder パターンで完全準拠。

---

## 🧠 深度逻辑与状态演进审查 (Deep Logic & State Evolution Audit)

### 执行路径脑内推演

**成功パス**: `IVKPsychePipeline.RunAsync(request)` → `VKPsycheContext` 生成（CorrelationId 自動付与） → `IVKPsychePipelineExecutor.ExecuteAsync` → **Before Stages**（Directive/Echo/Persona が並列 → Knowledge → Pattern → Weaving: Formatter → Truncate → CoordinateResolve → Replacement → TapestryWeaving） → **Middleware Chain**（Onion wrap → `InvokeChatEngineAsync` で LLM 呼び出し） → **After Stages** → `context.Response.Build()` で immutable `VKPsycheResponse` 構築 → `VKResult.Success(response)` 返却。

✅ 状態変異の伝播は `VKPsycheContext` の mutable プロパティ（`Response.Messages`, `_fragments`, `_states`）経由で確実に最終呼び出し元に到達。`VKPsycheResponseBuilder.Build()` で immutable snapshot に変換される設計は Clean Architecture の原則に合致。

**失敗パス**: いずれかの Before Stage で `VKResult.IsFailure` → `PsychePipelineRunner.ExecuteChunksAsync` が即座に `Failure` を返却 → `DefaultPsychePipelineExecutor` が `BehaviorsDiagnostics.ExecutionFailed` ログ → `VKResult.Failure<VKPsycheResponse>(errors)` 返却。

✅ エラー伝播経路に漏れなし。Middleware 失敗時も同パターンで安全に伝播。

### 逻辑死胡同 (Dead Ends)

1. **`VKWeavingOptions.MaxTokenLimit`**: 前回指摘で「未使用」だったが、今回は `DefaultPromptTruncateTask.cs:L43` で `context.Args<VKWeavingArgs>()?.MaxTokenLimit ?? _options.MaxTokenLimit` として参照され、L47 で `totalLimit = Math.Min(totalLimit, maxTokenLimit)` として `TotalContextLimit` の上限クランプに使用。**前回の Dead Config 指摘は解消済み。** ✅

2. **`VKEchoOptions.MaxWindowSize`**: 前回指摘で「未使用」だったが、今回は `DefaultEchoStage.cs:L74-L78` で `context.Args<VKEchoArgs>()?.MaxWindowSize ?? _echoOptions.MaxWindowSize` として参照され、スライディングウィンドウ制限が実装。**前回の Dead Config 指摘は解消済み。** ✅

3. **`VKBehaviorsDiagnostics.Metrics.PipelineDuration` / `.ExecutionDuration`**: メトリクス定数が定義されているが、実際の `Histogram<double>` / `Counter` 計測コードは存在しない。`Stopwatch` による計測はログ出力のみ。Dead Config が残存。

4. **`DefaultPromptTruncateTask._timeProvider`**: [L19](/src/BuildingBlocks/AI.Psyche/Weaving/Internal/DefaultPromptTruncateTask.cs) で `TimeProvider?` フィールドが宣言され [L34](/src/BuildingBlocks/AI.Psyche/Weaving/Internal/DefaultPromptTruncateTask.cs) で DI から受け取るが、**メソッド内のどこからも参照されていない**。Dead Field。

### 防御性逆向思考 (Destructive Thinking)

**潜在的バグ 1 — InMemoryKnowledgeStore.Seed の KeyNotFoundException**:
[InMemoryKnowledgeStore.cs:L38](/src/BuildingBlocks/AI.Psyche/Knowledge/Internal/InMemoryKnowledgeStore.cs) の `_store[groupKnowledge.Key.ToString()].Add(knowledgeEntry)` および L47 の `_store[groupKnowledge.Key.ToString()].AddRange(...)` は、キーが存在しない場合に `KeyNotFoundException` をスローする。`ConcurrentDictionary` の `[]` アクセサは `TryGetValue` ではなく直接アクセスのため、初回 Seed 時に確実に例外が発生する。`_store.GetOrAdd(key, _ => new List<VKKnowledgeEntry>())` パターンへの修正が必須。

**潜在的バグ 2 — PsychePipelineRunner の Abort 時 Success 返却**:
[PsychePipelineRunner.cs:L73-L76](/src/BuildingBlocks/AI.Psyche/Behaviors/Pipeline/Internal/PsychePipelineRunner.cs) で `context.IsAborted || cancellationToken.IsCancellationRequested` の場合に `VKResult.Success()` を返却している。キャンセルされたにもかかわらず Success を返すと、呼び出し元が部分的に処理されたコンテキストを成功と解釈する危険がある。`VKResult.Failure(VKBehaviorsErrors.Aborted)` または `OperationCanceledException` のスローが適切。

---

## 💡 改善ロードマップ (Evolutionary Roadmap)

### 1. 最優先対応 (Immediate Action)

| 優先度 | 対象 | アクション |
|:-------|:-----|:-----------|
| 🔴 High | InMemory Stores 5 ファイル (11 箇所) | `throw new ArgumentException` → `VKGuard.NotEmptyGuid(id)` に置換 (CS.01) |
| 🔴 High | [InMemoryKnowledgeStore.cs](/src/BuildingBlocks/AI.Psyche/Knowledge/Internal/InMemoryKnowledgeStore.cs) L38, L47 | `_store[key].Add()` → `_store.GetOrAdd(key, _ => []).Add()` で KeyNotFoundException を回避 |
| 🟡 Medium | [DefaultPsychePipelineExecutor.cs](/src/BuildingBlocks/AI.Psyche/Behaviors/Pipeline/Internal/DefaultPsychePipelineExecutor.cs) L166 | `new VKError(...)` → `VKBehaviorsErrors.ChatEngineNotFound` 定数に集約 |
| 🟡 Medium | [PsychePipelineRunner.cs](/src/BuildingBlocks/AI.Psyche/Behaviors/Pipeline/Internal/PsychePipelineRunner.cs) L75, L101 | Abort/Cancelled 時の `VKResult.Success()` → `VKResult.Failure` または `OperationCanceledException` |

### 2. リファクタリング提案 (Refactoring)

| 対象 | 提案 |
|:-----|:-----|
| `DefaultPromptTruncateTask._timeProvider` | 未使用フィールド。削除するか、将来的な TTL ベーストランケーションに使用する場合はコメントで意図を明記 |
| `VKWeavingStepRunner` の VK prefix | AP.03 準拠のため `WeavingStepRunner` に改名（internal クラス）。前回指摘と同じ |
| `VKPsycheContext._states` | `ConcurrentDictionary<Type, object>` への変更で並列 Before Stage からの SetState をスレッドセーフ化 |
| `VKBehaviorsDiagnostics.Metrics` | `Histogram<double>` / `Counter` を `DefaultPsychePipeline` と `DefaultPsychePipelineExecutor` に追加して OpenTelemetry Metrics 統合 |
| `PsychePipelineRunner` と `VKWeavingStepRunner` | 極めて類似したチャンク化+並列実行ロジック。共通基底ヘルパーへの統合を検討 |

### 3. 推奨される学習トピック (Learning Suggestions)

- **OpenTelemetry Metrics 統合**: `VKBehaviorsDiagnostics.Metrics` 定数が用意されているため、`System.Diagnostics.Metrics` の `Histogram<double>` を追加してパイプライン計測を実現
- **ConcurrentDictionary Best Practices**: InMemory Store の `GetOrAdd` / `AddOrUpdate` パターンの正しい使用法
- **Middleware Pattern Deep Dive**: ASP.NET Core の `IMiddleware` パターンとの比較。現在の Onion wrap は正しいが、`IAsyncDisposable` スコープ管理の考慮
