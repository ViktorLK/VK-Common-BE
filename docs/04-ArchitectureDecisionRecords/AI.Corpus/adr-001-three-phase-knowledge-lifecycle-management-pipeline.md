# ADR 001: Three-Phase Knowledge Lifecycle Management Pipeline

- **Date**: 2026-06-13
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/AI.Corpus

## 1. Context (背景)

複雑な AI 対話パイプラインやロールプレイ、動的コンテキスト切り替えを伴う業務シナリオにおいて、プロンプトへの知識（Knowledge）の動的注入は、大モデルの応答精度に極めて重大な影響を与える。単にベクトルストアから類似度順で知識を取得して差し込むだけでは、「現在話している話題（トピック）の持続」「発言のクールダウン」「依存する前提知識の自動解決」「感情や確率に基づく制限」といった高度なライフサイクル制御に対応できず、プロンプトが肥大化したり、対話の一貫性が損なわれたりする。

## 2. Problem Statement (問題定義)

単純な知識検索と無制限な注入（Naive Retrieval-Injection）には、以下の問題がある：
1. **コンテキストの不安定化**: 毎ターンで知識が現れたり消えたり（フラッピング）することで、大モデルの短期記憶が混乱し、発言の一貫性が著しく損なわれる。
2. **トークンバジェットの浪費**: 優先度の低い知識や無関係なコンテキストまで一律に注入されるため、トークン消費が跳ね上がる。
3. **注入状態の非永続化**: どの知識が実際に注入され、どの知識が冷却（Cooldown）状態にあるのかといった「履歴と状態」を追跡・保存する標準的な仕組みがなく、状態管理コードが複雑化する。

## 3. Decision (決定事項)

高度な知識制御とライフサイクル管理を標準化するため、新設の **`AI.Corpus` モジュール**において**「Three-Phase Knowledge Lifecycle Pipeline (3段階ナレッジライフサイクルパイプライン)」**を採用する。

1. **3段階のライフサイクル処理の確定**:
   - パイプラインを「Gathering (収集)」「Filtering (選別)」「Tracking (追跡)」の3フェーズに明確に分離する。
2. **Gathering (収集フェーズ - Before Stage)**:
   - `IVKRecallKnowledgeLifecycleStore` によるベクトルストアからの動的検索（Recall）と、`IVKStaticKnowledgeLifecycleStore` による事前登録された静的ルールのバッチ取得を行い、知識候補（`VKKnowledgeLifecycleEntry`）を収集する。
3. **Filtering (選別フェーズ - Before Stage)**:
   - 収集された候補群に対し、後述する 17 種のライフサイクルフィルター（`IVKKnowledgeLifecycleFilter`）を順次実行し、現在のコンテキスト（ターン数、感情状態、残トークンバジェット）に合わせて最適な知識のみを厳選する。
4. **Tracking (追跡フェーズ - After Stage)**:
   - LLM 呼び出し完了後、`KnowledgeUsageRecordStage` にて実際にプロンプトに注入された知識を検出し、その使用履歴（注入されたターン数、回数など）を `IVKKnowledgeUsageStore` に保存して永続化する。

### 核心的なパイプライン設計と概念図

```
Gathering (Before) ---> Filtering (Before) ---> LLM Execution ---> Tracking (After)
 [召回 & 静的取得]         [17種ルール評価]       [コア処理実行]      [注入履歴の記録]
```

```csharp
namespace VK.Blocks.AI.Corpus;

// 3段階それぞれの抽象化ステージの実装
internal sealed class DefaultGatheringStage : IVKPsycheBeforePipelineStage { ... }
internal sealed class CorpusFilteringStage : IVKPsycheBeforePipelineStage { ... }
internal sealed class KnowledgeUsageRecordStage : IVKPsycheAfterPipelineStage { ... }
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: Monolithic Knowledge Stage inside AI.Psyche
- **Approach**: `AI.Psyche` の `DefaultKnowledgeStage` の中に、取得、フィルタ、保存のすべてのコードをモノリシックに記述する。
- **Rejected Reason**: フィルタの種類（17種）やストレージ（永続化）の要件が非常に複雑であり、これらを Psyche に詰め込むと SRP（単一責任原則）を大きく逸脱し、Psyche の保守性が壊滅するため。

### Option 2: Stateful Actor-based Lifecycle Management
- **Approach**: ユーザーセッションごとに Akka.NET などのアクター（Actor）を立ち上げ、メモリ上で知識のステートマシン（Cooldown/Sticky）を自律駆動させる。
- **Rejected Reason**: システム構成が非常に複雑になり、マルチノードでの水平スケールアウトやサーバーレス環境（Azure Functions 等）での実行互換性が失われるため。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **抜群のコンテキスト安定性**: Cooldown や Stickiness などのルールにより、一度出現した知識が適切に数ターン維持されるため、LLM が文脈を失わずに自然な対話を継続できる。
- **高い関心の分離**: プログラマーは Psyche のレンダリング処理と、Corpus のライフサイクル制御処理を完全に独立して開発・テストできる。

### Negative
- **処理ステップ数の増加**: 3つの独立したステージが Psyche のパイプラインに挟まるため、DI の解決数と実行時呼び出しのオーバーヘッドが僅かに増加する。

### Mitigation
- ストアやトラッキング処理の書き込み操作には非同期バックグラウンド実行を許容し、メインスレッド（レスポンスタイムのホットパス）をブロックしないように工夫する。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **Audit Trail**: `Tracking` 段階で保存される `KnowledgeUsageRecord` は、ユーザー発言の文脈と AI がどのルールを採用したかの証跡となるため、システムのハラスメント対策や監査時の重要なトレーサビリティ情報として活用する。

## 7. Status
✅ Accepted
