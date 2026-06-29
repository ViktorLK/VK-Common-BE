# ADR 012: Semantic Cache Integration

- **Date**: 2026-06-03
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/AI.SemanticKernel

## 1. Context (背景)

エンタープライズ向けの AI アプリケーションや社内問い合わせアシスタントにおいて、ユーザーから送信される質問には「パスワードのリセット方法は？」「有給申請のルールは？」といった、意味的に同一または極めて類似したクエリが繰り返し発生する。これらの定型的な質問に対して都度外部の LLM API を呼び出すことは、無駄な API 課金の発生と、不要なネットワークおよび推論によるレイテンシの蓄積を引き起こす。

## 2. Problem Statement (問題定义)

伝統的なキャッシュシステム（Redis 等の厳密キー一致型キャッシュ）を AI の質問応答に適用する手法には、以下の問題がある：
1. **表記揺れに極端に弱い**: 自然言語入力では、「パスワードリセットのやり方は？」「PWの変更手順」「パスワードを再設定したい」といった表現の揺れがあるため、厳格な文字列一致（Exact Match）のキャッシュではヒット率が著しく低くなる。
2. **キャッシュインジェクション**: 類似の質問に対するキャッシュ内容に誤りや古い情報が含まれていた場合、ユーザーに誤った回答が高速に返され続け、手動でキャッシュをクリアする手段がシステム的に整備されていないと障害が長期化する。
3. **アロケーションの無駄**: キャッシュのルックアップ処理自体に重いリフレクションや複雑なデータモデルのデシリアライズが伴うと、高負荷環境での GC ボトルネックが新たな課題となる。

## 3. Decision (決定事項)

自然言語クエリの意味的な合致度に基づいて高速にキャッシュを返却するため、**「Semantic Cache (ベクトル類似度セマンティックキャッシュ)」**を採用する。

1. **`AISKSemanticCache` の実装**:
   - `IVKSemanticCache` インターフェース（またはコアのキャッシュモデル）に基づき、`AI.SemanticKernel` 内で `AISKSemanticCache` を具現化する。
2. **ベクトル相似度によるルックアップアルゴリズム**:
   - 新規クエリ受信時、まず `IVKEmbeddingEngine` を用いてクエリをベクトル（Embedding）に変換する。
   - ベクトルデータベース（`IVKVectorStore` 等）に対してベクトル検索を実行し、最も近い過去のクエリレコードを探す。
   - 類似度スコア（コサイン類似度など）が、事前に設定されたしきい値 `MinSimilarityScore`（例: `0.92`）以上である場合、キャッシュヒット（Hit）と判定し、過去に保存された応答データを即時に返却して LLM 呼び出しをバイパスする。
3. **ヒットしたキャッシュの非破壊的更新**:
   - キャッシュがヒットした場合、対応するキャッシュエントリの TTL（生存期間）を延長する。ヒットしなかった場合は、LLM から返ってきた応答を非同期でベクトル化してキャッシュストアに登録（Upsert）する。

### 核心的なセマンティックキャッシュの実装

```csharp
namespace VK.Blocks.AI.SemanticKernel.SemanticCache.Internal;

internal sealed class AISKSemanticCache : IVKSemanticCache
{
    private readonly IVKEmbeddingEngine _embeddingEngine;
    private readonly IVKVectorStoreReader _vectorStoreReader;
    private readonly IVKVectorStoreWriter _vectorStoreWriter;
    private readonly AISKSemanticCacheOptions _options;

    public async Task<VKResult<VKCacheEntry?>> GetAsync(string query, CancellationToken ct = default)
    {
        VKGuard.NotNullOrWhiteSpace(query);

        // 1. クエリのベクトル化
        var embeddingResult = await _embeddingEngine.GenerateEmbeddingAsync(query, ct).ConfigureAwait(false);
        if (embeddingResult.IsFailure) return VKResult.Failure<VKCacheEntry?>(embeddingResult.Errors);

        // 2. ベクトルストアからの最近傍検索
        var searchResult = await _vectorStoreReader.SearchAsync(
            embeddingResult.Data, 
            limit: 1, 
            minScore: _options.MinSimilarityScore, 
            ct).ConfigureAwait(false);

        if (searchResult.IsSuccess && searchResult.Data.Count > 0)
        {
            var match = searchResult.Data[0];
            return VKResult.Success<VKCacheEntry?>(new VKCacheEntry
            {
                Query = match.Text,
                Response = match.ResponseContent,
                Score = match.Score
            });
        }

        return VKResult.Success<VKCacheEntry?>(null); // キャッシュミス
    }
}
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: Keyword-based Cache (Inverted Index)
- **Approach**: Elasticsearch などの転置インデックスを利用し、入力された名詞キーワードの共通度（TF-IDF / BM25）でキャッシュヒットを判定する。
- **Rejected Reason**: 同義語（例: 「有給」と「年休」）や文脈の意味合い（否定表現など）の差異を正確に処理できず、誤った回答をキャッシュから返してしまうリスクが高いため。

### Option 2: Middleware-level Interception with RedisVL
- **Approach**: Redis 独自のベクトルライブラリである RedisVL を直接インフラに組み込んでキャッシュを委ねる。
- **Rejected Reason**: インフラが Redis に完全にロックインされてしまうため、AWS OpenSearch や Azure Cosmos DB をベクトルストアとして利用する環境においてセマンティックキャッシュ機能が動作しなくなるため。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **劇的なコスト削減**: よくある質問（FAQ）のヒットにより、LLM API 呼び出し回数が最大 40〜60% 削減され、従量課金コストが削減される。
- **極低レイテンシ応答**: LLM の推論（数秒）をスキップし、ミリ秒単位でキャッシュが返却されるため、ユーザー体験が著しく向上する。

### Negative
- **冷たいキャッシュへの初回ペナルティ**: キャッシュミス時、クエリのベクトル化（Embedding生成）と、大モデルへの推論（ChatGeneration）が連続して発生するため、初回のレスポンスタイムが若干遅くなる。

### Mitigation
- 高速なオンプレミス/ローカルの極小埋め込みモデル（ONNX / local BERT 等）を Embedding 登録に選択できるようにし、ベクトル化処理の遅延を 5ms 以下に抑え込む。

## 6. Implementation & Security (実装详细とセキュリティ考察)

- **Tenant Isolation**: キャッシュレコードのインデックスには、クエリのベクトルデータに加えて `TenantId` 属性を強制的にフィルタリング条件として付与し、別テナントの質問応答キャッシュが他テナントへ漏洩するのを物理的に防ぐ（マルチテナントの絶対隔離）。
- **Data Expiration (TTL)**: キャッシュされた回答が古いままで残り続けないよう、すべてのベクトルレコードには有効期限（TTL）メタデータを付与し、定期的に自動クリーンアップされる設計を適用する。

## 7. Status
✅ Accepted
