# ADR 006: Parallel Chunk Weaving Pipeline

- **Date**: 2026-05-31
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/AI.Psyche

## 1. Context (背景)

プロンプト構築（Weaving）のライフサイクルにおいて、実行すべきステージ（Stage）には順序制約がある（例：会話履歴を取得する前に、ユーザー入力を処理しておく必要がある等）。一方で、I/O処理や外部ストレージへのクエリを実行する複数の独立したステージ（例：ペルソナ取得と知識ベース照合）を同期的に1つずつ実行することは、システム全体のレスポンスタイムの悪化に直接繋がる。

## 2. Problem Statement (問題定義)

単純な順序実行や制御の甘い並行処理設計には、以下の問題がある：
1. **シーケンシャル遅延の蓄積**: 各ステージが最大100msを消費する場合、4つのステージを順次実行すると合計で400ms以上かかり、LLM呼び出し前のオーバーヘッドとしては許容できない。
2. **依存関係の制御不能**: 完全に並行実行してしまうと、順序に依存するステージ（例：すべてのテキストフラグメントが集まった後に実行すべきフォーマッターやトランケーター）が不完全な状態で動作してしまう。
3. **エラー発生時のリソース浪費**: ある先行ステージが権限エラーなどで失敗（`Result.Failure`）しているにもかかわらず、無関係な後続の並行タスクが走り続けてしまい、リソースやAPIクォータを無駄に消費する。

## 3. Decision (決定事項)

効率的な実行順序制御と高スループットな並行処理を両立するため、**「Parallel Chunk Weaving Pipeline (並行チャンク編排パイプライン)」**を採用する。

1. **実行順序（`StageOrder`）と並行グループ（`ParallelGroup`）の宣言**:
   - `IVKWeavingStage` に優先順序インデックスおよび並行実行をグループ化する識別子を設ける。
2. **`VKWeavingStepRunner` による実行チャンクの動的構築**:
   - 登録された有効なステージ群を読み込み、同じ優先順序および並行グループに属するステージ同士を「並行実行可能なチャンク」としてグループ化し、異なるグループ間はシーケンシャルな順番で繋ぐ。
3. **`Task.WhenAll` と Fail-Fast 制御**:
   - 並行チャンク内の処理は `Task.WhenAll` で並行処理し、全体の待ち時間をチャンク内最大遅延時間に抑える。
   - チャンクの実行中、先行グループでエラー（`VKResult.Failure`）が発生した場合、後続グループの処理実行は直ちにスキップ（Fail-Fast）する。

### 核心的なパイプライン実行ロジック

```csharp
namespace VK.Blocks.AI.Psyche;

internal static class VKWeavingStepRunner
{
    public static List<List<T>> ChunkSteps<T>(
        IEnumerable<T> steps,
        Func<T, int> getOrder,
        Func<T, int> getGroup)
    {
        // StageOrder と ParallelGroup の組み合わせでソートし、チャンク化する
        return steps
            .GroupBy(s => new { Order = getOrder(s), Group = getGroup(s) })
            .OrderBy(g => g.Key.Order)
            .Select(g => g.ToList())
            .ToList();
    }

    public static async Task ExecuteChunksAsync<T>(
        List<List<T>> chunks,
        VKWeavingContext context,
        Func<T, bool> isParallel,
        Func<T, VKWeavingContext, CancellationToken, Task<VKResult>> executeStep,
        Func<VKWeavingContext, bool> checkContinue,
        Action<VKWeavingContext, VKResult> onError,
        CancellationToken ct)
    {
        foreach (var chunk in chunks)
        {
            if (!checkContinue(context)) break;

            if (chunk.Count > 1 && chunk.All(isParallel))
            {
                // チャンク内の全タスクを並行実行
                var tasks = chunk.Select(async step =>
                {
                    var res = await executeStep(step, context, ct).ConfigureAwait(false);
                    if (!res.IsSuccess)
                    {
                        onError(context, res);
                    }
                });
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            else
            {
                // シーケンシャル実行
                foreach (var step in chunk)
                {
                    var res = await executeStep(step, context, ct).ConfigureAwait(false);
                    if (!res.IsSuccess)
                    {
                        onError(context, res);
                        break;
                    }
                }
            }
        }
    }
}
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: Monolithic Orchestration Code
- **Approach**: `DefaultPsychePipeline` 内で直接、各ステージの具現クラスをハードコードし、必要に応じて並行タスクを作成して待つ。
- **Rejected Reason**: ステージの増減や順序の入れ替えが発生するたびに、オーケストレーターコードを手動で書き換える必要があり、拡張性（OCP）を大きく阻害するため。

### Option 2: Full Directed Acyclic Graph (DAG) Solver
- **Approach**: 完全な有向非巡回グラフ（DAG）解析エンジンを組み込み、ステージごとの依存関係をトポロジカルソートして実行する。
- **Rejected Reason**: 実装コストが極めて高く、プロンプト組装に必要な数ステップの実行に対して、オーバーキルな設計（複雑すぎるデータ構造）となるため。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **最小遅延の達成**: `Echo`（履歴取得）と `Knowledge`（知識照合）など、お互いに独立したステージが並行して実行されるため、レスポンスタイムが大幅に向上する。
- **安全な中断**: パイプライン実行途中で発生したエラー（例：不適切なペルソナ指定）が即時に検知され、無駄な後続タスクを起動せずに上位にエラーを返却できる。

### Negative
- **非同期コンテキストでの例外ハンドリング**: `Task.WhenAll` 内部で発生した個々の未処理例外（Unhandled Exception）のトレースが複雑になる。

### Mitigation
- パイプライン内の各ステージ処理は、例外を直接リークさせず、各ステージ内部で捕捉し、`VKResult` に変換した上で上位へ返却するように安全策を徹底する（CS.01 に準拠）。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **ConfigureAwait(false)**: 図書館（Library）コードとしての共通ルールに則り、すべての await 呼び出しに `.ConfigureAwait(false)` を強制し、ASP.NET Core などの同期コンテキストにおけるデッドロックを防止する（CS.03 に準拠）。
- **CancellationToken Propagation**: すべての非同期タスクに `CancellationToken` を強制的に引き回し、上位からキャンセルが指示された際は直ちに実行を中止して処理をクリーンアップする。

## 7. Status
✅ Accepted
