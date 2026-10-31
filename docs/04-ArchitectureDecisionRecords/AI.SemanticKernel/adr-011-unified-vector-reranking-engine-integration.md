# ADR 011: Unified Vector ReRanking Engine Integration

- **Date**: 2026-06-03
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/AI.SemanticKernel

## 1. Context (背景)

RAG (Retrieval-Augmented Generation / 検索拡張生成) において、ベクトルデータベースからコサイン類似度などの距離基準のみで検索された初期の文書フラグメント（ドキュメントチャンク）は、キーワードの一致や局所的な類似性だけで判断される。そのため、本当にユーザーの質問の意図（セマンティクス）に最も適した情報が上位に配置されず、余計なノイズ文書が Prompt に混入して生成品質の低下やトークン数の無駄遣いが発生する。

## 2. Problem Statement (問題定義)

初期のベクトル検索結果をそのまま Prompt 構築に回す手法には、以下の問題がある：
1. **ノイズ情報の混入**: 類似度スコアが高いものの、文脈的・全体的な意味では質問に答えていないテキストチャンクが上位に残り、LLM の指示追従の妨げになる（"Lost in the Middle" 現象）。
2. **トークンコストの膨大化**: 関連度の低い文書を大量に Prompt に埋め込むことで、API の入力トークン数が不要に増加し、ランニングコストが高騰する。
3. **ベンダー依存の密結合**: Cohere Rerank などの特定の重配（ReRanker）プロバイダーの SDK をドメインロジックに直接実装すると、重配プロバイダーを別のモデル（ローカルの Cross-Encoder 等）へ切り替える際に大きな改修が発生する。

## 3. Decision (決定事項)

RAG 検索結果の精度向上と軽量なプロバイダー抽象化を実現するため、コアに **`IVKReRankerEngine` インターフェース**を導入し、**「Unified Vector ReRanking Engine (統一ベクトル重配エンジン)」**を統合する。

1. **`IVKReRankerEngine` の共通定義**:
   - `VK.Blocks.AI` に `IVKReRankerEngine` を定義する。
   - 入力としてクエリ文字列と候補ドキュメント群を受け取り、関連度スコアを再計算してソートしたドキュメントリストを `VKResult` で返す設計とする。
2. **`AI.SemanticKernel` での重配エンジンの具現**:
   - `AISKReRankerEngine` を実装する。外部の ReRank モデル（例: Cohere, HuggingFace Cross-Encoder 等）への接続を抽象化し、初召回されたリストをフィルタリング及び並び替えする。
3. **NoOp フォールバックの提供**:
   - リレーショナルな重配モデルが未定義・無効化されている環境のために `NoOpAISKReRankerEngine`（スコア計算をせず、そのままの順番でスルーパスする無害なダミー実装）を提供し、システム起動とテストの安定性を保つ。

### 核心的な ReRank エンジンのインターフェースと実装

```csharp
namespace VK.Blocks.AI;

public interface IVKReRankerEngine
{
    Task<VKResult<IReadOnlyList<VKReRankedDocument>>> ReRankAsync(
        string query,
        IEnumerable<VKDocumentChunk> documents,
        int? topN = null,
        CancellationToken cancellationToken = default);
}

namespace VK.Blocks.AI.SemanticKernel.ReRanking.Internal;

internal sealed class AISKReRankerEngine : IVKReRankerEngine
{
    // 重配プロバイダーのSDKをカプセル化し、二段階ソートを実行
    public async Task<VKResult<IReadOnlyList<VKReRankedDocument>>> ReRankAsync(
        string query,
        IEnumerable<VKDocumentChunk> documents,
        int? topN = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(query);
        VKGuard.NotNull(documents);

        try
        {
            var rawDocs = documents.ToList();
            if (rawDocs.Count == 0) return VKResult.Success<IReadOnlyList<VKReRankedDocument>>([]);

            // 外部の ReRank API やモデルを呼び出してスコアを算出
            var scoredDocs = await ExecuteExternalReRankModelAsync(query, rawDocs, cancellationToken).ConfigureAwait(false);

            var ordered = scoredDocs
                .OrderByDescending(d => d.Score)
                .Take(topN ?? rawDocs.Count)
                .ToList();

            return VKResult.Success<IReadOnlyList<VKReRankedDocument>>(ordered);
        }
        catch (Exception ex)
        {
            return VKResult.Failure<IReadOnlyList<VKReRankedDocument>>(
                new VKError("AI.ReRanking.Failed", $"ReRanking execution failed: {ex.Message}"));
        }
    }
}
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: LLM-based Re-ranking (LLM as a Judge)
- **Approach**: 別の高速な LLM（例: GPT-4o-mini など）を呼び出し、プロンプトで「以下の文書の中で質問に関連する順番に並び替えてください」と指示する。
- **Rejected Reason**: LLM 自体の推論遅延とコストが余分に発生し、ミリ秒単位の速度が要求される検索クエリの応答時間制限を突破してしまうため。

### Option 2: Database-side Native Re-ranking
- **Approach**: pgvector や Azure AI Search が備える組み込みのハイブリッドセマンティックランク機能を直接利用する。
- **Rejected Reason**: データベース側のライセンスや機能に強く縛られるため、シンプルなインメモリベクトルストアや簡易的なローカルベクター検索環境において重配処理を実行できなくなるため。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **Prompt 品質の大幅な向上**: ReRank フェーズを通すことで、本当に価値のある文脈のみが上位 N 個（Top-N）に絞られてプロンプトに注入されるため、LLM の回答精度が極めて高くなる。
- **Token 節約**: 関連度の低い下位のドキュメントをフィルタリングで切り捨てられるため、入力トークンサイズが最適化される。

### Negative
- **追加の API 呼出遅延**: 外部の ReRank サービス（Cohere 等）へネットワーク要求を送るため、検索全体のレイテンシが数十ミリ秒増加する。

### Mitigation
- 高速なオンプレミス型（ローカル CPU/GPU 駆動）の ReRank エンジン実装をインフラレベルで選択可能にする、または `topN` 値を適切に絞って処理データ量をコントロールする。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **API Token Security**: Cohere などの外部重配 API キーは、`VKRetrievalOptions` などの構成パターンにラップし、他のエンジン同様にログ出力時に PII Masking で完全に隠蔽する。
- **Null Safety**: 入力ドキュメントコレクションが空の場合や、クエリ文字列がホワイトスペースのみの場合は、外部要求を送信せずに即座に空のリストまたは失敗の結果を返却する防衛コードを実装する。

## 7. Status
✅ Accepted
