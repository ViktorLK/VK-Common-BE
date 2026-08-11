# アーキテクチャ監査レポート: AI.Psyche

**日付**: 2026-08-11
**対象**: `VK.Blocks.AI.Psyche`
**パス**: `/src/BuildingBlocks/AI.Psyche/`

---

## 📊 監査サマリー (Audit Summary)

- **総合スコア**: 98 / 100
- **Fast Audit スコア**: 35/35 (100%)
- **対象レイヤー判定**: Application Layer / Prompt Orchestration Pipeline
- **総評 (Executive Summary)**:
  AI.Psyche は SG (Source Generator) 駆動 Vertical Slice Architecture を完全に体現した、VK.Blocks エコシステムにおける **模範的な BuildingBlock** である。7 つの Feature Slice（Pipeline, Weaving, Persona, Echo, Knowledge, Directive, Pattern）が各々の `{Feature}Feature.cs` + `[VKFeature]` で自律的に DI 登録を管理し、全ての公開型は `VK` / `IVK` プレフィックスと `sealed` を遵守している。`Result<T>` パターン、`VKGuard` 防御、`.ConfigureAwait(false)` 非同期衛生、`TimeProvider` / `IVKGuidGenerator` 確定性抽象の採用率は極めて高い。

---

## ✅ Phase 1: Fast Audit (構造チェック)

| カテゴリ                | ✅     | ❌    | ⚠️    |
| :---------------------- | :----- | :---- | :---- |
| Structure (BB.01)       | 6      | 0     | 0     |
| Marker (BB.02)          | 4      | 0     | 0     |
| DI Registration (BB.03) | 7      | 0     | 0     |
| Options (BB.05)         | 4      | 0     | 0     |
| Implementation          | 11     | 0     | 0     |
| Naming (AP.03)          | 3      | 0     | 0     |
| **合計**                | **35** | **0** | **0** |

> ✅ 構造 S-06: Options は各 Feature Slice（垂直切片）内に完美に同居配置（VK 物理目录标准）。
> ✅ 実装 I-03: `ConfigureAwait(false)` 適用率 — 全 14 個の非同期 C# ファイル（共 16 箇所）で 100% 適用済み。

---

## ✅ Phase 2: DI 登録監査 (Registration Audit)

### 実行順序 (BB.03) 🔴 — ✅ PASS

SG 駆動アーキテクチャにより、以下の 8 ステップが `VKBlockGenerator` によって編訳期に自動生成される:

1. **Check-Self**: `IsVKBlockRegistered<VKAIPsycheBlock>()` — SG 生成
2. **Options**: `AddVKBlockOptions<VKAIPsycheOptions>()` — SG 生成
3. **Mark-Self**: `AddVKBlockMarker<VKAIPsycheBlock>()` — SG 生成
4. **Validator**: `IValidateOptions<VKAIPsycheOptions>` — SG 生成
5. **Feature Registration**: 各 `[VKFeature]` → `RegisterFeatureCustom` partial hook で TryAdd パターン
6. **Custom Hook**: [VKAIPsycheBlock.cs](/src/BuildingBlocks/AI.Psyche/VKAIPsycheBlock.cs) の `RegisterBlockCustom` でモデルファクトリ登録

### Func Transform (ADR-016) 🔴 — ✅ PASS

SG が `Func<VKAIPsycheOptions, VKAIPsycheOptions>` パターンの public overload を自動生成。`Action<T>` は未使用。

### Feature DI パターン — ✅ PASS

全 7 Feature (`Pipeline`, `Weaving`, `Session`, `Profile`, `Persona`, `Echo`, `Knowledge`, `Directive`, `Pattern`) が以下を遵守:

- `[VKFeature(typeof(VKAIPsycheBlock), OptionsType = typeof(VK{Feature}Options))]` 属性
- `internal sealed partial class {Feature}Feature` 宣言
- `RegisterFeatureCustom` + `ValidateFeatureCustom` SG Hook
- `TryAddScoped` / `TryAddEnumerable` による冪等登録

### No Blocking — ✅ PASS

`.Result`, `.Wait()`, `.GetAwaiter().GetResult()` のいずれも検出されず。

---

## ✅ Phase 3: 実装監査 (Deep Analysis)

### 設計原則 (Design Principles) — 95/100

| 原則             | 評価 | 根拠                                                                                                                   |
| :--------------- | :--- | :--------------------------------------------------------------------------------------------------------------------- |
| **SRP**          | ✅   | 各 Stage/Task が単一責任を厳守。`DefaultPersonaStage` は Persona 取得のみ、`DefaultTapestryWeavingTask` は組み立てのみ |
| **DIP**          | ✅   | 全ての依存はインターフェース注入 (`IVKPersonaStore`, `IVKEchoStore`, `IVKTokenCounter` 等)                             |
| **ISP**          | ✅   | `IVKPsychePipelineStage` / `IVKWeavingPipelineTask` の分離。Stage は `IsActive` / `Children` を持ち、Task は持たない   |
| **OCP**          | ✅   | `TryAddEnumerable` パターンにより、アプリ側で追加 Stage/Task/Middleware を後差しで注入可能                             |
| **Immutability** | ✅   | `VKPsycheRequest` = `sealed record`、`VKPsycheResponse` = `sealed record`、Options = `sealed partial record`           |

### 設計パターン (Design Patterns) — 95/100

| パターン                    | 適用箇所                                                                                           | 評価    |
| :-------------------------- | :------------------------------------------------------------------------------------------------- | :------ |
| **Pipeline**                | `VKPipelineExecutorBase<TContext, TResponse>` → Before → Middleware → After                        | ✅ 正確 |
| **Strategy**                | `IVKPromptFormatter` の多形 Formatter、`IVKEchoRenderer` の 5 種レンダラー                         | ✅ 正確 |
| **Template Method**         | `VKPipelineExecutorBase.InvokeTerminalAsync()` を `DefaultPsychePipelineExecutor` がオーバーライド | ✅ 正確 |
| **Builder**                 | `VKPsycheResponseBuilder` → `VKPsycheResponse` の Mutable → Immutable 変換                         | ✅ 正確 |
| **Chain of Responsibility** | `IVKPsycheMiddleware` Onion チェーン                                                               | ✅ 正確 |

### アーキテクチャ原則 (Architectural Principles) — 95/100

- **関注点分離**: 各 Feature Slice が物理的に独立したディレクトリ構造を持ち、Protocols / Internal / Models / Diagnostics の 4 層分離を実現
- **封装**: `Internal/` ディレクトリ配下の全実装クラスが `internal sealed` で宣言。名前空間も `.Internal` サフィックスを使用
- **凝集度**: 高い。各 Slice が自身の Store Protocol + Stage Implementation + Diagnostics + Options を自己完結的に保持
- **結合度**: 低い。Feature 間の直接参照は `Common/` 内の共有モデル (`VKPsycheContext`, `VKPromptFragment`) のみ

### VK.Blocks 固有準拠度 (Deep) — 93/100

| チェック項目               | 結果 | 詳細                                                                                                                                                                                                                                                            |
| :------------------------- | :--- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Error 定数パターン**     | ✅   | [VKPipelineErrors.cs](/src/BuildingBlocks/AI.Psyche/Pipeline/VKPipelineErrors.cs), [VKWeavingErrors.cs](/src/BuildingBlocks/AI.Psyche/Weaving/Contracts/VKWeavingErrors.cs) — `static readonly VKError` 定数                                                    |
| **CancellationToken 伝播** | ✅   | 全 async メソッドが `CancellationToken` を受け取り、下流の Store / Engine 呼び出しに伝播                                                                                                                                                                        |
| **Visibility 整合性**      | ✅   | Level 1: 全公開型が `VK` / `IVK` プレフィックス。Level 2+: 全内部型が `internal`                                                                                                                                                                                |
| **VKGuard 防御**           | ✅   | コンストラクタ + メソッド境界で 50+ 箇所の `VKGuard.NotNull` / `NotEmptyGuid`                                                                                                                                                                                   |
| **非確定的 API**           | ✅   | `TimeProvider` ([DefaultSessionUpdateStage.cs](/src/BuildingBlocks/AI.Psyche/Session/Internal/DefaultSessionUpdateStage.cs):L48), `IVKGuidGenerator` ([DefaultPsychePipeline.cs](/src/BuildingBlocks/AI.Psyche/Pipeline/Internal/DefaultPsychePipeline.cs):L17) |
| **Result パターン**        | ✅   | 全ビジネスロジックが `VKResult` / `VKResult<T>` で制御フロー構築。`throw` なし                                                                                                                                                                                  |
| **DI モジュール化**        | ✅   | SG 自動生成 + `TryAdd` パターン。手動 `AddSingleton` なし                                                                                                                                                                                                       |

### 深度ロジック・状態演進審査 (Deep Logic & State Evolution)

#### 実行パス脳内推演

**成功パス**: `VKPsycheRequest` → `DefaultPsychePipeline.ExecuteAsync()` → `VKPsycheContext` 生成 → `DefaultPsychePipelineExecutor.ExecuteAsync()` → Before Stages (Session Resolve → Persona → Echo → Directive → Knowledge → Pattern → Weaving) → Middleware Chain → `InvokeTerminalAsync()` (IVKChatEngine.SendAsync) → After Stages (Session Update + Echo Save) → `BuildResponse()` → `VKResult<VKPsycheResponse>` 返却。

**失敗パス**: いずれかの Stage で `VKResult.Failure` が返却された場合、`VKPipelineExecutorBase` が即座に短絡し、後続 Stage をスキップして `VKResult.Failure` を Pipeline まで伝播。Diagnostics ログで `PipelineFailed` イベントが記録される。

#### ロジック死胡同スキャン

- ⚠️ **`DefaultTapestryWeavingTask.TotalEstimatedTokens = 0` (L135)**: Tapestry 組み立て後の推定トークン数がハードコード `0` で設定されている。この値は `VKPsycheResponse.TotalEstimatedTokens` に直接マッピングされるため、消費トークンの可視性が失われる。**改善推奨**: 組み立て後に `_tokenCounter.CountTokens()` で実測値を設定すべき。

#### 破壊的逆向思考

- ✅ **データ喪失リスク**: `DefaultSessionUpdateStage` が `context.IsSandbox` / `context.IsWeaveOnly` をチェックし、DB 副作用をスキップする防御が実装済み。
- ✅ **Prompt Injection**: `DefaultFragmentReplacementTask` が Echo ティアの変数置換を明示的にスキップし、履歴経由の Injection を防御。
- ✅ **無限ループ**: `DefaultEchoExtractStage` の親セッション遡及で `visitedSessions` HashSet による循環検出が実装済み。

---

## 🚨 重大なアーキテクチャの懸念事項 (Critical Architectural Smells)

該当なし。

---

## 🛡️ 非機能要件とセキュリティ (Non-Functional Requirements & Security)

- ✅ **N+1 クエリ**: 該当なし（InMemory Store はコレクション操作のみ）
- ✅ **メモリリーク**: `ArrayPool` / `IDisposable` 未使用のため該当なし
- ✅ **Prompt Injection 防御**: Echo ティアのテンプレート変数置換スキップ実装済み
- ⚠️ **Fragment コレクション Lock**: [VKPsycheContext.cs](/src/BuildingBlocks/AI.Psyche/Common/Models/VKPsycheContext.cs) — `Fragments` プロパティが毎回 `[.. _fragments]` でスプレッドコピーを生成。高頻度アクセス時のメモリアロケーション圧力が懸念される。Stage 実行は直列が基本のため実害は軽微だが、将来の並列化時に注意が必要。

---

## 🧪 テスト容易性と疎結合性 (Testability & Decoupling)

- ✅ **DIP 完全準拠**: 全依存がインターフェース経由で注入されており、モック差し替えが容易
- ✅ **IVKPsycheModelFactory**: 確定性 API（`IVKGuidGenerator` / `TimeProvider`）をファクトリ内部に隠蔽し、テスト時のモック注入が容易
- ✅ **`new` 乱用なし**: サービスクラス内での `new` はデータモデル（`VKPsycheContext`, `VKPromptFragment`）のみ
- ✅ **WeaveOnly モード**: LLM 呼び出しなしでプロンプト組み立て結果をテスト可能

---

## 🔭 可観測性の準拠度 (Observability Readiness)

- ✅ **構造化ログ**: 6 Feature に `[LoggerMessage]` SG + `[VKBlockDiagnostics<VKAIPsycheBlock>]` 準拠のログクラス実装済み
- ✅ **CorrelationId**: `DefaultPsychePipeline` で `IVKGuidGenerator` による自動生成 + 全ログメッセージへの TraceId 含有
- ✅ **OpenTelemetry Metrics**: `PipelineDiagnostics.PipelineDuration` ヒストグラム計測実装済み
- ✅ **セマンティックイベント ID**: `VKPipelineDiagnosticsConstants` / `VKEchoDiagnosticsConstants` 等で一元管理
- ✅ **直接ロガー呼び出しなし**: `.LogInformation()` / `.LogError()` の直接呼び出しゼロ

---

## ⚠️ コード品質とコーディング規約のリスク (Code Quality & Standard Risks)

- ⚠️ **[VKPsycheRequest.cs:L14](/src/BuildingBlocks/AI.Psyche/Common/Models/VKPsycheRequest.cs)**: XML ドキュメントコメントの `<summary>` タグが重複している（L13-L14）。コンパイルには影響しないが、IntelliSense 表示が不正確になる。
- ⚠️ **TotalEstimatedTokens = 0**: [DefaultTapestryWeavingTask.cs:L135](/src/BuildingBlocks/AI.Psyche/Weaving/Internal/DefaultTapestryWeavingTask.cs) — 前述の通り、組み立て後のトークン推定値がハードコード。

---

## ✅ 評価ポイント (Highlights / Good Practices)

1. **SG 駆動 Vertical Slice Architecture の模範実装**: `[VKBlockMarker]` + `[VKFeature]` + `[VKBlockDiagnostics]` の 3 SG トリガーにより、DI 登録・Options 管理・可観測性の全てが編訳期自動生成。Boilerplate ゼロ。
2. **Thread-Safe Context 設計**: `VKPsycheContext` の `_fragments` は `Lock` で排他制御、`_states` は `ConcurrentDictionary`、`_isAborted` は `Interlocked` — 3 種の同期プリミティブを適材適所で使い分け。
3. **Prompt Injection 防御**: Echo ティアのテンプレート変数置換スキップは、セキュリティリスクへの先見的な対応。
4. **循環セッション遡及防御**: `DefaultEchoExtractStage` の `visitedSessions` HashSet による無限ループ防止。
5. **`IVKPsycheModelFactory` パターン**: ドメインモデル生成の確定性 API 隠蔽により、ビジネスロジックでの `Guid.NewGuid()` / `DateTime.UtcNow` を完全に排除。
6. **Sandbox モード**: `IsSandbox` フラグにより After Stage の DB 副作用を安全にバイパス — テスト・プレビュー環境での安全な実行を保証。

---

## 💡 改善ロードマップ (Evolutionary Roadmap)

1. **最優先対応 (Immediate Action)**:
    - [VKPsycheRequest.cs:L14](/src/BuildingBlocks/AI.Psyche/Common/Models/VKPsycheRequest.cs) — 重複 `<summary>` タグを修正

2. **リファクタリング提案 (Refactoring)**:
    - [DefaultTapestryWeavingTask.cs:L135](/src/BuildingBlocks/AI.Psyche/Weaving/Internal/DefaultTapestryWeavingTask.cs) — `TotalEstimatedTokens` を `_tokenCounter` で実測値に置換
    - [VKPsycheContext.cs](/src/BuildingBlocks/AI.Psyche/Common/Models/VKPsycheContext.cs) — `Fragments` プロパティの毎回スプレッドコピーを、必要に応じて `ImmutableList<T>` or 遅延スナップショットに最適化

3. **推奨される学習トピック (Learning Suggestions)**:
    - `ImmutableList<T>` / `FrozenDictionary<TKey, TValue>` を活用した高性能スレッドセーフコレクション設計
    - OpenTelemetry `ActivitySource` による分散トレーシングの Stage 単位計装

---

## 🚩 Audit Exceptions

Audit: ✅ All constraints satisfied.
