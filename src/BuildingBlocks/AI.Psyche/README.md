# VK.Blocks.AI.Psyche

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)](#)

## はじめに

`VK.Blocks.AI.Psyche` は、LLM ベースの AI チャットアプリケーション向けに設計された**プロンプトオーケストレーション・パイプライン**です。

Persona（人格定義）、Echo（対話履歴）、Knowledge（動的ナレッジ）、Directive（テナント指令）の 4 つの情報源を統合的に管理し、トークン予算制約の下で最適なプロンプトを自動組み立てする「**Prompt Weaving（プロンプト織り込み）**」エンジンを提供します。

### 設計思想

- **Zero-Infrastructure InMemory Default**: 開発・テスト環境ではインフラ依存なしで即座に動作。全ストアが InMemory 実装をデフォルトで提供
- **Pluggable Store Architecture**: `IVKPersonaStore` / `IVKEchoStore` / `IVKKnowledgeStore` / `IVKDirectiveStore` の DI 差し替えのみで永続化層に移行可能
- **Parallel Stage Execution**: 独立した抽出ステージ（Persona / Echo / Directive）を `ParallelGroup` による宣言的並列実行で高スループットを実現
- **Token-Aware Truncation**: 対話履歴のトークン予算管理をエンジン内部で自動化し、コンテキストウィンドウの溢れを防止
- **Expression Tree Compilation**: Knowledge エントリのキーワード / 正規表現マッチングを Expression Tree にコンパイルし、`ConcurrentDictionary` でキャッシュ

---

## アーキテクチャ

### 適用パターン

| カテゴリ                   | パターン                                                                                   |
| -------------------------- | ------------------------------------------------------------------------------------------ |
| **Design Principles**      | SRP, DIP, ISP, Fail-Fast, Immutability                                                     |
| **Design Patterns**        | Strategy, Pipeline, Chain of Responsibility, Template Method (Generics), Builder (Fluent)   |
| **Architectural Patterns** | Vertical Slice (Feature-Driven), Options Pattern, Result Pattern                            |
| **Enterprise Patterns**    | Token Budget Management, Dialogue History Pruning, Expression Tree Compilation & Caching    |
| **Cross-Cutting**          | Source Generated Logging, `VKGuard` Boundary Defense, Thread-Safe Context, Func Transform   |

### プロンプト織り込みフロー

```mermaid
flowchart TB
    subgraph Input ["入力"]
        REQ[/"VKWeavingRequest<br/>(TenantId, PersonaId,<br/>SessionId, UserInput)"/]
    end

    subgraph Pipeline ["IVKWeavingPipeline"]
        REQ --> CTX["VKWeavingContext 生成<br/>(CorrelationId 自動付与)"]

        CTX --> CHUNK["VKWeavingStepRunner<br/>ChunkSteps()"]

        subgraph Extraction ["Extraction Stage (ParallelGroup=1)"]
            direction LR
            PERSONA["Persona Stage<br/>• PersonaAnchor 取得<br/>• System Prompt 生成"]
            ECHO["Echo Stage<br/>• 対話履歴取得<br/>• Turn/Message Pruning<br/>• Token Budget 適用"]
            DIRECTIVE["Directive Stage<br/>• テナント指令取得<br/>• System 指示注入"]
        end

        subgraph KnowledgePhase ["Knowledge Stage (ParallelGroup=2)"]
            KNOWLEDGE["Knowledge Stage<br/>• タイムライン構築<br/>• キーワード/正規表現マッチ<br/>• Cooldown/Sticky 判定<br/>• Exclusive Group 解決"]
        end

        CHUNK --> Extraction
        Extraction --> KnowledgePhase
    end

    subgraph Weaving ["Weaving Stage"]
        KnowledgePhase --> FORMAT["FormatterTask (300)<br/>• IVKPromptFormatter<br/>• Fragment → Content"]
        FORMAT --> TRUNC["TruncateTask (400)<br/>• Token Budget 適用<br/>• History Eviction"]
        TRUNC --> TAPESTRY["TapestryWeavingTask (500)<br/>• System 統合<br/>• Timeline 組立<br/>• Depth Injection"]
    end

    subgraph Output ["出力"]
        TAPESTRY --> RESULT["VKResult&lt;VKPromptTapestry&gt;<br/>(Messages, SystemInstructions)"]
    end

    style REQ fill:#4a9eff,color:#fff
    style RESULT fill:#22c55e,color:#fff
```

### モジュール構成

```
AI.Psyche/
├── Common/                        # 横断的関心事
│   ├── Contracts/                 # 公開データモデル (VKWeavingRequest, VKPromptTapestry)
│   ├── DependencyInjection/       # Block-level DI (VKPsycheBlockExtensions, IVKPsycheBuilder)
│   │   └── Internal/             # BlockRegistration, BlockBuilder
│   └── Shared/                   # 共有基盤 (VKWeavingContext, VKPromptFragment, VKWeavingStepRunner)
├── Directive/                     # テナント指令機能
│   ├── Diagnostics/Internal/     # [LoggerMessage] SG Diagnostics
│   ├── Internal/                 # DefaultDirectiveStage, InMemoryDirectiveStore, DirectiveFeature
│   ├── Models/                   # VKDirectiveCharter
│   └── Protocols/                # IVKDirectiveStore, IVKDirectiveOptions
├── Echo/                          # 対話履歴（短期記憶）機能
│   ├── Diagnostics/Internal/     # [LoggerMessage] SG Diagnostics
│   ├── Internal/                 # DefaultEchoStage, InMemoryEchoStore, EchoRenderers, EchoFeature
│   ├── Models/                   # VKEchoTrace, VKEchoPruneUnit
│   └── Protocols/                # IVKEchoStore, IVKEchoRenderer, IVKEchoOptions, IVKEchoOverrides
├── Knowledge/                     # 動的ナレッジ機能
│   ├── Diagnostics/Internal/     # [LoggerMessage] SG Diagnostics
│   ├── Internal/                 # DefaultKnowledgeStage, InMemoryKnowledgeStore, KnowledgeFeature
│   ├── Models/                   # VKKnowledgeEntry, VKKnowledgeKey, Position, FilterLogic, Trigger
│   └── Protocols/                # IVKKnowledgeStore, IVKKnowledgeRenderer, IVKKnowledgeOptions
├── Persona/                       # ペルソナ（人格定義）機能
│   ├── Diagnostics/Internal/     # [LoggerMessage] SG Diagnostics
│   ├── Internal/                 # DefaultPersonaStage, InMemoryPersonaStore, PersonaFeature
│   ├── Models/                   # VKPersonaAnchor, VKOutputSpecification, VKFewShotExample
│   └── Protocols/                # IVKPersonaStore, IVKPersonaRenderer, IVKPersonaOptions
├── Pipeline/                      # パイプライン統合
│   ├── Diagnostics/Internal/     # [LoggerMessage] SG Diagnostics
│   ├── Internal/                 # DefaultPsychePipeline, PipelineFeature
│   └── Protocols/                # IVKWeavingStage, IVKPresetProvider
├── Weaving/                       # プロンプト織り込みエンジン
│   ├── Diagnostics/Internal/     # [LoggerMessage] SG Diagnostics
│   ├── Internal/                 # FormatterTask, TruncateTask, TapestryTask, WeavingEngine
│   └── Protocols/                # IVKWeavingTask, IVKWeavingTaskEngine, IVKPromptFormatter
├── VKAIPsycheBlock.cs             # [VKBlockMarker] ブロックマーカー
└── VKAIPsycheOptions.cs           # ルート Options (IVKBlockOptions)
```

---

## 主な機能

### 🧠 Persona（ペルソナ管理）

- **構造化ペルソナ定義**: `VKPersonaAnchor` による名前・説明・性格特性・システム指示・出力仕様・Few-Shot 例の統合管理
- **Pluggable Renderer**: `IVKPersonaRenderer` による Markdown ベースの構造化レンダリング。カスタムフォーマットへの差し替えが可能
- **出力仕様制御**: `VKOutputSpecification` による JSON Schema / 言語コード / トークンヒント / カスタム制約の宣言的指定
- **動的ペルソナ切替**: `AllowDynamicPersonaSwitching` オプションによるランタイムペルソナ切り替え対応

### 💬 Echo（対話履歴管理）

- **デュアルプルーニング戦略**: `VKEchoPruneUnit.Turn`（ターン単位）と `Message`（メッセージ単位）の切り替え
- **動的トークンバジェット**: `TokenBudgetRatio`（全体コンテキストに対する比率）と `MaxTokens`（絶対上限）のデュアル制約
- **Turn Budget 制限**: `MaxTurns` による対話ターン数の上限制御
- **System Message フィルタリング**: `IncludeSystemMessages` による対話内システムメッセージの取捨選択
- **マルチレンダラー**: Default / ChatML / XML / Bracket の 4 つの履歴レンダリング形式を標準提供

### 📚 Knowledge（動的ナレッジ注入）

- **3 種のトリガータイプ**: `Constant`（常時有効）/ `Keyword`（キーワードマッチ）/ `Regex`（正規表現）
- **Expression Tree コンパイル**: キーワード / 正規表現ルールを `System.Linq.Expressions` でコンパイルし、`ConcurrentDictionary` でキャッシュ。ReDoS 防御の Regex タイムアウト（100ms）付き
- **高度なフィルタリング**: `AndAll` / `AndAny` / `NotAny` / `NotAll` の 4 種の論理演算子による複合条件マッチ
- **Cooldown / Sticky / Delay**: ターン単位のクールダウン、スティッキー（持続）、ディレイ（遅延発火）制御
- **Exclusive Grouping**: 同一グループ内の競合エントリを Weight ベースで自動解決
- **位置制御**: `AbsolutePosition`（グローバル Depth 挿入）と `RelativePosition`（他ティアとの相対配置）の 2 種

### 📋 Directive（テナント指令）

- **テナント分離**: `TenantId` ベースの指令解決により、マルチテナント環境での System Prompt カスタマイズを実現
- **Scoped Store**: `InMemoryDirectiveStore` は `Scoped` ライフタイムで登録され、リクエスト間の分離を保証

### 🧵 Weaving Engine（プロンプト織り込みエンジン）

- **3 段階処理パイプライン**: Formatter（メタデータ → テキスト変換） → Truncation（トークン予算適用） → Tapestry Assembly（最終メッセージ列組立）
- **宣言的ティア無効化**: `DisabledTiers` による Persona / Echo / Knowledge / Directive の選択的無効化
- **レンダー順序オーバーライド**: `TierRenderOrderOverrides` によるティア配置順の動的変更
- **Depth Injection**: SillyTavern スタイルの Absolute Position 挿入（グローバル Depth ベースのメッセージ差し込み）

### ⚡ VKWeavingStepRunner（汎用並列実行エンジン）

- **ジェネリック設計**: Stage と Task の両方で再利用される `ChunkSteps<T>` + `ExecuteChunksAsync<T>`
- **宣言的並列制御**: `ParallelGroup` と `StageOrder` / `TaskOrder` による同一レイヤーの並列実行とレイヤー間の直列実行
- **Fail-Fast コールバック**: `shouldContinueFunc` / `onFailureAction` による即時中断と柔軟なエラーハンドリング

### 📡 可観測性 (Observability)

- **Source Generated Logging**: 全 Feature に `[LoggerMessage]` SG + `[VKBlockDiagnostics<VKAIPsycheBlock>]` 準拠の構造化ログ
- **CorrelationId トレーシング**: パイプライン開始時に `IVKGuidGenerator` で自動生成し、全ログメッセージに含める
- **セマンティックイベント ID**: 各 Feature の公開 Diagnostics 定数（`VKPipelineDiagnostics`, `VKEchoDiagnostics` 等）でイベント ID を一元管理
- **パイプライン計測**: 開始 / 完了 / 失敗イベントの自動ログ出力、`Stopwatch` による実行時間計測

---

## 採用技術

| 技術                                       | 用途                                                           |
| ------------------------------------------ | -------------------------------------------------------------- |
| **.NET 10 / C# 13**                        | ランタイム基盤、`sealed record`、`required`、Primary Constructor |
| **System.Linq.Expressions**                | Knowledge Matcher の高性能 Expression Tree コンパイル           |
| **ConcurrentDictionary**                   | コンパイル済みマッチャーのスレッドセーフキャッシュ              |
| **System.Threading.Lock**                  | `VKWeavingContext` の Fragment コレクション排他制御             |
| **IOptions / IOptions\<T\>**               | Feature 別の構成管理                                           |
| **Source Generator**                       | `[LoggerMessage]` SG, `[VKBlockDiagnostics]` SG, `[VKFeature]` SG |
| **VK.Blocks.Core**                         | Result Pattern, VKGuard, DI Builder, Block Options, IVKGuidGenerator |
| **VK.Blocks.AI**                           | `IVKTokenCounter`, `VKChatRole`, `VKChatMessage` 等の AI 共通基盤 |

---

## 開始方法

### 1. パッケージ参照

```xml
<ProjectReference Include="..\AI.Psyche\VK.Blocks.AI.Psyche.csproj" />
```

### 2. DI 登録

```csharp
builder.Services
    .AddVKPsycheBlock(builder.Configuration)
    .AddVKDefaultFeatures();   // Directive + Echo + Knowledge + Persona + Pipeline + Weaving を一括有効化
```

#### 個別 Feature 登録（選択的構成）

```csharp
builder.Services
    .AddVKPsycheBlock(builder.Configuration)
    .AddVKPersona()
    .AddVKEcho(options => options with { TokenBudgetRatio = 0.4, PruneUnit = VKEchoPruneUnit.Turn })
    .AddVKKnowledge(options => options with { MaxEntriesToInject = 10, SemanticThreshold = 0.8f })
    .AddVKDirective()
    .AddVKPipeline()
    .AddVKWeaving(options => options with { TotalContextLimit = 65536, MaxResponseTokens = 4096 });
```

### 3. パイプライン実行

```csharp
public sealed class ChatService(IVKWeavingPipeline pipeline)
{
    public async Task<VKResult<VKPromptTapestry>> BuildPromptAsync(
        string tenantId, string personaId, string sessionId, string userInput,
        CancellationToken ct = default)
    {
        var request = new VKWeavingRequest
        {
            TenantId = tenantId,
            PersonaId = personaId,
            SessionId = sessionId,
            UserInput = userInput,
            Args = new VKWeavingArgs
            {
                TotalContextLimit = 32768,
                MaxResponseTokens = 2048
            }
        };

        return await pipeline.WeaveTapestryAsync(request, ct);
    }
}
```

### 4. ストアのカスタマイズ

```csharp
// InMemory デフォルトを永続化ストアに差し替え
builder.Services.AddSingleton<IVKPersonaStore, PostgresPersonaStore>();
builder.Services.AddSingleton<IVKKnowledgeStore, CosmosKnowledgeStore>();
builder.Services.AddScoped<IVKEchoStore, RedisEchoStore>();
builder.Services.AddScoped<IVKDirectiveStore, DatabaseDirectiveStore>();
```

> [!TIP]
> `TryAdd` パターンにより、カスタムストアの登録は `AddVKDefaultFeatures()` **よりも前**に行う必要があります。先に登録されたサービスが優先されます。

---

## 🏛️ アーキテクチャ監査

最新の監査レポートは [AI.Psyche_20260531.md](/docs/04-AuditReports/AI.Psyche/AI.Psyche_20260531.md) を参照してください。

| 項目             | 結果                  |
| ---------------- | --------------------- |
| **総合スコア**   | 91 / 100              |
| **Fast Audit**   | 18/18 (100%)          |
| **DI Registration** | ✅ PASS (BB.03 完全準拠) |
| **重大な懸念事項** | なし                  |

### 監査による改善提案（軽微）

- `VKWeavingErrors.FormatterNotFound` の Description を `"demo"` から本番用メッセージに更新
- `VKWeavingContext._evicted` コレクションの Lock 保護追加
- インライン `VKError.Failure(...)` 4 箇所の定数化

---

## 🔭 今後の展望

| 機能                              | 状態 | 概要                                                    |
| --------------------------------- | :--: | ------------------------------------------------------- |
| **Prompt Weaving Pipeline**       |  ✅  | 4 ティア統合パイプラインの実装完了                        |
| **Expression Tree Matching**      |  ✅  | Knowledge キーワード / 正規表現の高性能マッチング          |
| **Parallel Stage Execution**      |  ✅  | ParallelGroup ベースの宣言的並列実行                      |
| **OpenTelemetry Metrics**         |  🔄  | `VKPipelineDiagnostics.Metrics` 定義済み、計測コード未実装 |
| **Semantic Knowledge Retrieval**  |  📋  | Vector Store 連携による意味検索ナレッジ取得               |
| **Incremental State Tracking**    |  📋  | Knowledge マッチング状態の増分追跡による大規模対応         |
| **Preset Provider**               |  📋  | `IVKPresetProvider` によるプリセットプロンプトテンプレート  |
| **Streaming Tapestry**            |  📋  | ストリーミング応答に対応した段階的 Tapestry 組み立て       |

---

## ライセンス

MIT License — 詳細は [LICENSE](/LICENSE) を参照してください。
