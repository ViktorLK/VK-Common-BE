# 🏗️ アーキテクチャ監査レポート — AI.Psyche

**対象モジュール**: `VK.Blocks.AI.Psyche`
**監査日**: 2026-05-31
**監査バージョン**: Full Architecture Audit (Phase 1–4)
**Audit**: ✅

---

## 📊 監査サマリー (Audit Summary)

- **総合スコア**: **91 / 100**
- **Fast Audit スコア**: 18/18 (100%) — 全チェック項目 PASS
- **対象レイヤー判定**: Application Layer / Prompt Orchestration Pipeline (BuildingBlock)
- **総評 (Executive Summary)**:
  AI.Psyche は VK.Blocks アーキテクチャ規約への準拠度が極めて高い、成熟したモジュールである。Vertical Slice 構成（Feature 単位）、`sealed` + `record` パターン、`Result<T>` フロー、`[LoggerMessage]` SG ロギング、`VKGuard` 境界防御、`ConfigureAwait(false)` 伝播、`TryAdd` 冪等 DI 登録など、Industrial DNA の全要件を満たしている。並列ステージ実行エンジン（`VKWeavingStepRunner`）はジェネリックで再利用可能な設計であり、ドメインモデル（Knowledge Matcher の Expression Tree コンパイル）も高度な実装品質を示す。減点要因は「軽微なインライン VKError 生成」「`DefaultTapestryWeavingTask` のコンストラクタ DI 弱依存」「`VKWeavingContext.Evicted` のスレッドセーフ性不足」「`KnowledgeLog` の命名規約軽微逸脱」に限定される。

---

## 🚨 重大なアーキテクチャの懸念事項 (Critical Architectural Smells)

_該当なし_ — レイヤー間の依存逆転、循環依存、致命的パフォーマンスボトルネックは検出されなかった。

---

## 🛡️ 非機能要件とセキュリティ (Non-Functional Requirements & Security)

- 🔒 **[スレッドセーフ性] [VKWeavingContext.cs](/src/BuildingBlocks/AI.Psyche/Common/Shared/VKWeavingContext.cs:L111-L126)**:
  `_fragments` コレクションは `Lock` で保護されているが、`_evicted` コレクションは **ロック保護なし** で `List<T>` に直接 `Add` している。複数ステージが並列実行される場合（`IsParallel = true`）、`AddEvicted` が同時呼び出しされると **データ競合** が発生する可能性がある。`_fragments` と同様の `Lock` 保護、または `ConcurrentBag<T>` への変更が推奨される。

- 🔒 **[DI 弱依存] [DefaultTapestryWeavingTask.cs](/src/BuildingBlocks/AI.Psyche/Weaving/Internal/DefaultTapestryWeavingTask.cs:L19-L25)**:
  コンストラクタで `IOptions<VKWeavingOptions>? options = null` と `ILogger? logger = null` を **nullable optional** として受け取り、`?? new VKWeavingOptions()` / `?? NullLogger.Instance` でフォールバックしている。これは CS.07（`GetRequiredService` デフォルト）に反し、DI 構成不備を隠蔽するリスクがある。ただし、テスト容易性を意図した設計の可能性もあるため Warn 扱いとする。

- 🔒 **[パフォーマンス注記] [VKKnowledgeMatcher.cs](/src/BuildingBlocks/AI.Psyche/Knowledge/VKKnowledgeMatcher.cs:L17-L23)**:
  `typeof(string).GetMethod(...)` による静的リフレクションは起動時 1 回のみ実行されるが、`ConcurrentDictionary` によるキャッシュ付き Expression Tree コンパイルは適切に実装されている。Regex タイムアウト（100ms）も ReDoS 防御として適切。問題なし。

---

## 🧪 テスト容易性と疎結合性 (Testability & Decoupling)

- ⚙️ **[優れた疎結合設計]**: 全ステージ実装が `IVKWeavingStage` インターフェース経由で DI に登録され、`TryAddEnumerable` によるマルチ実装パターンを採用。テスト時のモック差し替えが容易。
- ⚙️ **[InternalsVisibleTo]**: `.csproj` で `VK.Blocks.AI.Psyche.UnitTests` と `DynamicProxyGenAssembly2` への公開が設定済み。Moq による internal クラスのモック化も可能。
- ⚙️ **[VKWeavingStepRunner]**: ジェネリック設計により、`Func<T,...>` デリゲート経由でテスタブル。テスト時にフェイク実行関数を注入できる。
- ⚙️ **[改善点]**: `DefaultTapestryWeavingTask` の optional DI パラメータは、テスト容易性を高めるが CS.07 規約との整合性が必要。

---

## 🔭 可観測性の準拠度 (Observability Readiness)

- 📡 **[OR.01 完全準拠]**: 全 Diagnostics クラスが `[LoggerMessage]` Source Generator パターンを使用。`logger.LogXxx()` 直呼び出しは **ゼロ件** 。
- 📡 **[VKBlockDiagnostics 属性]**: 全 Feature の Diagnostics クラスに `[VKBlockDiagnostics<VKAIPsycheBlock>]` が付与され、BB.04 準拠。
- 📡 **[DiagnosticConstants パターン]**: 各 Feature に公開 Diagnostics 定数クラス（`VKPipelineDiagnostics`, `VKEchoDiagnostics`, `VKWeavingDiagnostics`, `VKKnowledgeDiagnosticTokens` 等）が存在し、EventId のセマンティックトークン管理が実現。
- 📡 **[CorrelationId 伝播]**: `DefaultPsychePipeline` が `IVKGuidGenerator` を使用して CorrelationId を自動生成し、ログメッセージに含めている。TraceId の明示的伝播は確認済み。
- 📡 **[軽微な注意点]**: `KnowledgeLog.cs` のクラス名が `KnowledgeLog` であり、他のモジュールの `XxxDiagnostics` 命名規約と一致していない。機能的影響はなし。

---

## ⚠️ コード品質とコーディング規約のリスク (Code Quality & Standard Risks)

### CS.01 — インライン VKError 生成

以下の箇所で `static readonly` 定数ではなく、インラインで `VKError.Failure(...)` を生成している：

| ファイル | 行 | エラーコード |
|:---------|:---|:------------|
| [DefaultPsychePipeline.cs](/src/BuildingBlocks/AI.Psyche/Pipeline/Internal/DefaultPsychePipeline.cs:L103-L104) | L103 | `Psyche.Pipeline.EmptyTapestry` |
| [DefaultPromptWeavingEngine.cs](/src/BuildingBlocks/AI.Psyche/Weaving/Internal/DefaultPromptWeavingEngine.cs:L70) | L70 | `Weaving.NoTapestry` |
| [DefaultTapestryWeavingTask.cs](/src/BuildingBlocks/AI.Psyche/Weaving/Internal/DefaultTapestryWeavingTask.cs:L64) | L64 | `Weaving.EmptyActive` |
| [DefaultKnowledgeStage.cs](/src/BuildingBlocks/AI.Psyche/Knowledge/Internal/DefaultKnowledgeStage.cs:L47) | L47 | `Knowledge.MissingPersona` |

**推奨**: 既存の `VKWeavingErrors` / `VKKnowledgeErrors` クラスに `static readonly VKError` 定数として集約すべき。

### VKWeavingErrors の記述品質

[VKWeavingErrors.cs](/src/BuildingBlocks/AI.Psyche/Weaving/VKWeavingErrors.cs:L10) のエラー定数 `FormatterNotFound` の Description が `"demo"` となっている。本番用の適切なメッセージへの更新が必要。

### BB.03 — DI 登録順序 (Block Level)

[VKPsycheBlockRegistration.cs](/src/BuildingBlocks/AI.Psyche/Common/DependencyInjection/Internal/VKPsycheBlockRegistration.cs) の BB.03 8ステップ検証：

| ステップ | 期待 | 実装 | 判定 |
|:---------|:-----|:-----|:-----|
| 1. Check-Self | `IsVKBlockRegistered<T>()` | ✅ L20 | PASS |
| 2. Options | `AddVKBlockOptions(...)` | ✅ L24 | PASS |
| 3. Mark-Self | `AddVKBlockMarker<T>()` | ✅ L26 | PASS |
| 4. Validator | SG 自動生成 | ✅ `[VKFeature]` 属性付き | PASS |
| 5. Diagnostics | SG 自動生成 | ✅ `[VKBlockDiagnostics]` 存在 | PASS |
| 6. Toggle (Enabled check) | `if (!options.Enabled) return` | ✅ L28 AFTER Mark | PASS |
| 7. Core Services | Feature 別登録 | ✅ Builder 経由 | PASS |

### BB.03 — Func Transform

`Func<VKAIPsycheOptions, VKAIPsycheOptions>` パターンを使用（`Action<T>` ではない）。✅ ADR-016 準拠。
全 Feature Extension も `Func<TOptions, TOptions>? transform` パターンを使用。✅

### AP.03 — Visibility 整合性

| レベル | 対象 | 可視性 | VK Prefix | 判定 |
|:-------|:-----|:-------|:----------|:-----|
| L1 (Public) | `IVKWeavingPipeline`, `VKWeavingRequest`, `VKPromptTapestry`, `VKWeavingContext` | `public` | ✅ VK prefix | PASS |
| L1 (Public) | `VKAIPsycheBlock`, `VKAIPsycheOptions` | `public` | ✅ VK prefix | PASS |
| L1 (Public) | `IVKWeavingStage`, `IVKEchoStore`, `IVKPersonaStore` 等 | `public` | ✅ VK prefix | PASS |
| L2+ (Internal) | `DefaultPsychePipeline`, `DefaultEchoStage` 等 | `internal` | ✅ No VK prefix | PASS |
| L2+ (Internal) | Feature Classes (`EchoFeature`, `DirectiveFeature` 等) | `internal` | ✅ No VK prefix | PASS |
| Exception | `VKWeavingStepRunner` | `internal` | ⚠️ VK prefix on internal | WARN |

**注**: `VKWeavingStepRunner` は `internal` だが `VK` prefix を持つ。AP.03 の L2+ ルール（internal は VK prefix なし）からの逸脱。Common/Shared のユーティリティクラスであるため影響は軽微。

---

## ✅ 評価ポイント (Highlights / Good Practices)

### 設計原則 (Design Principles)

- **SRP 徹底**: 各 Feature（Directive, Echo, Knowledge, Persona, Weaving, Pipeline）が独立したフォルダに分離され、単一責任の境界が明確。
- **DIP 完全準拠**: 全依存関係がインターフェース経由（`IVKWeavingStage`, `IVKEchoStore`, `IVKPersonaStore`, `IVKKnowledgeStore`, `IVKDirectiveStore`, `IVKPromptFormatter`, `IVKWeavingTaskEngine`）。具象クラスへの直接依存ゼロ。
- **Result<T> パターン**: `throw` 文 **ゼロ件**。全エラーフローが `VKResult<T>` で統一され、`IsFailure` チェック → `VKResult.Failure(errors)` 伝播パターンが徹底。

### 設計パターン (Design Patterns)

- **Strategy パターン**: `IVKPromptFormatter` による Feature 別フォーマット戦略（Persona, Echo, Knowledge, Directive）。`CanFormat` ガードによる動的ディスパッチ。
- **Pipeline / Chain of Responsibility**: `IVKWeavingStage` → `IVKWeavingTask` の 2 層パイプライン。`StageOrder` / `TaskOrder` による宣言的実行順序制御。
- **Template Method (via Generics)**: `VKWeavingStepRunner.ExecuteChunksAsync<T>` がジェネリック関数デリゲートでチャンク化実行を抽象化。Stage と Task の両方に再利用。
- **Observer (Diagnostics)**: `[LoggerMessage]` SG による構造化ログでパイプラインのライフサイクルイベントを通知。

### アーキテクチャ原則

- **関心点分離**: 抽出（Stage） → フォーマット → トランケーション → 組立（Tapestry）の 4 段階処理が明確に分離。
- **高凝集・低結合**: Knowledge の `VKKnowledgeMatcher` は Expression Tree コンパイルをカプセル化し、`ConcurrentDictionary` キャッシュを内部に持つ自己完結型。
- **スレッドセーフ設計**: `VKWeavingContext._fragments` の `Lock` 保護、並列ステージ実行時のスレッドセーフなフラグメント収集。

### VK.Blocks 固有準拠度

- **CS.06**: `IVKGuidGenerator` を使用（`DefaultPsychePipeline` L49）。`DateTime.UtcNow` / `Guid.NewGuid()` 直呼びゼロ。
- **CS.03**: 全 `await` に `.ConfigureAwait(false)` 付与。`CancellationToken` が全 async チェーンで途切れなく伝播。
- **AP.01**: 全クラスが `sealed`。全 Options が `sealed record`。`required` キーワード使用。`VKGuard` 境界防御が全 public メソッドエントリポイントに配置。
- **BB.02**: `[VKBlockMarker(Dependencies = [typeof(VKAIBlock)])]` が正しく宣言。依存関係グラフが明示的。
- **BB.05**: 全 Options が `sealed record` + `init`。`[VKFeature]` 属性で SG 連携。
- **BB.06**: Feature Pattern 完全準拠。`AddVKDirective()` → `DirectiveFeature.Register()` のチェーン。`IVKAIPsycheBuilder` 経由のビルダーパターン。
- **DL.05**: 全 Feature クラスに `// [SG Hook]` タグ付きの `static partial void RegisterCustom` / `ValidateCustom` メソッド。

---

## 🧠 深度逻辑与状态演进审查 (Deep Logic & State Evolution Audit)

### 执行路径脑内推演

**成功パス**: `WeaveTapestryAsync(request)` → `VKWeavingContext` 生成 → Stages を `StageOrder` でソート → `ChunkSteps` でチャンク化 → Extraction ステージ（Directive/Echo/Persona が ParallelGroup=1 で並列、Knowledge が ParallelGroup=2 で次に実行） → Weaving ステージ（DefaultWeavingStage → DefaultPromptWeavingEngine → FormatterTask → TruncateTask → TapestryWeavingTask） → `context.Tapestry` にアセンブル → `VKResult.Success(tapestry)` 返却。

✅ 状態変異の伝播は `VKWeavingContext` の mutable プロパティ（`Tapestry`, `_fragments`）経由で確実に最終呼び出し元に到達。

**失敗パス**: いずれかのステージで `VKResult.IsFailure` → `onFailureAction` コールバック → `hasFailed = true` → `shouldContinueFunc` が `false` → ループ中断 → `failedResult.Errors` が `VKResult.Failure<VKPromptTapestry>` として返却。

✅ エラー伝播経路に漏れなし。`failedResult != null` のダブルチェックも堅牢。

### 逻辑死胡同 (Dead Ends)

1. **`VKWeavingOptions.MaxTokenLimit`**: 宣言（L15: `public int MaxTokenLimit { get; init; } = 32768;`）されているが、**モジュール内のどこからも参照されていない**。`TotalContextLimit` との関係が不明。未使用プロパティの疑い。

2. **`VKEchoOptions.MaxWindowSize`**: バリデーション（`EchoFeature.ValidateCustom`）は存在するが、`DefaultEchoStage` の実装内で **実際のスライディングウィンドウ制限として使用されていない**。Token Budget と Turn Budget のみが適用されている。宣言されて検証されるが効力を持たない Dead Config。

3. **`VKPipelineDiagnostics.Metrics.PipelineDuration`**: メトリクス定数が定義されているが、**実際の Metrics 計測コード（Counter/Histogram）は存在しない**。`Stopwatch` による計測はログ出力のみ。OpenTelemetry Metrics への統合が未実施。

### 防御性逆向思考 (Destructive Thinking)

**潜在的バグ**: `VKWeavingContext.Evicted` のスレッドセーフ性不足（前述）。並列 Extraction ステージ（ParallelGroup=1）が同時に `AddEvicted` を呼び出す場合、`List<T>.Add` は非スレッドセーフであり、データ破損の可能性がある。ただし、現在の実装では `AddEvicted` は `DefaultPromptTruncateTask`（TaskOrder=400, 直列実行）からのみ呼ばれるため、**現行の実行フローでは安全** だが、将来のカスタムステージ追加時にリスクとなる設計上の脆弱性。

---

## 💡 改善ロードマップ (Evolutionary Roadmap)

### 1. 最優先対応 (Immediate Action)

| 優先度 | 対象 | アクション |
|:-------|:-----|:-----------|
| 🔴 High | [VKWeavingErrors.cs](/src/BuildingBlocks/AI.Psyche/Weaving/VKWeavingErrors.cs:L10) | `FormatterNotFound` の Description を `"demo"` から本番用メッセージに修正 |
| 🟡 Medium | [VKWeavingContext.cs](/src/BuildingBlocks/AI.Psyche/Common/Shared/VKWeavingContext.cs:L111-L126) | `_evicted` コレクションに `Lock` 保護を追加（`_fragments` と同等のパターン） |
| 🟡 Medium | インライン VKError 4 箇所 | `VKWeavingErrors` / `VKKnowledgeErrors` に `static readonly` 定数として集約 |

### 2. リファクタリング提案 (Refactoring)

| 対象 | 提案 |
|:-----|:-----|
| `DefaultTapestryWeavingTask` コンストラクタ | nullable optional DI → `VKGuard.NotNull` 必須 DI に統一。テスト用途は `IOptions<T>` のモックで対応 |
| `VKWeavingOptions.MaxTokenLimit` | 未使用なら削除。使用意図があるなら `DefaultPromptTruncateTask` に統合 |
| `VKEchoOptions.MaxWindowSize` | `DefaultEchoStage` に実際のスライディングウィンドウロジックを追加、または options から削除 |
| `KnowledgeLog.cs` クラス名 | `KnowledgeDiagnostics` と統合、または `KnowledgeLogDiagnostics` に改名して命名規約統一 |
| `VKWeavingStepRunner` の VK prefix | AP.03 準拠のため `WeavingStepRunner` に改名（internal クラス） |

### 3. 推奨される学習トピック (Learning Suggestions)

- **OpenTelemetry Metrics 統合**: `VKPipelineDiagnostics.Metrics.PipelineDuration` 定数が用意されているため、`System.Diagnostics.Metrics` の `Histogram<double>` を `DefaultPsychePipeline` に追加してパイプライン実行時間の計測を実現
- **Incremental State Tracking**: Knowledge Stage のパフォーマンスコメント（L104-L115）で示唆されている「増分状態追跡」パターンの実装検討
- **Span/ArrayPool**: `DefaultPersonaRenderer.cs` の `StringBuilder` → `Span<char>` / `ArrayPool<char>` への最適化検討（CS.04 準拠度向上）
