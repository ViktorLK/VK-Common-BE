# Extension Guide: VectorSearch Subsystem

- **Scope**: `VK.Blocks.VectorSearch`
- **Specification Type**: Custom Stage & Reranker Implementation Guide (`ARCH-11`, `AP.06`)
- **Status**: ✅ Active

---

## 1. 🔌 カスタム PipelineStage の実装手順

パイプラインは Composite パターンで編成されており、`IVKSearchStage` を実装して DI 登録することで、独自の処理ステップを検索フローへ差し込むことができます。

### 1.1 ゴールデンコード例: カスタム Reranker Stage

```csharp
namespace VK.Blocks.VectorSearch.Custom;

public sealed class CustomRerankStage(IVKReranker reranker) : IVKSearchStage
{
    public int Order => 500; // RRF 融合の後、Context 圧縮の前に実行

    public async ValueTask<VKResult<VKSearchContext>> ProcessAsync(
        VKSearchContext context,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        // 候補が空の場合はスキップ
        if (context.Candidates.Count == 0)
        {
            return VKResult<VKSearchContext>.Success(context);
        }

        var rerankResult = await reranker.RerankAsync(
            context.Request.Query,
            context.Candidates,
            topN: context.Request.TopK,
            cancellationToken).ConfigureAwait(false);

        if (!rerankResult.IsSuccess)
        {
            // 縮退方針: リランキング失敗時は元の順位を維持して継続
            return VKResult<VKSearchContext>.Success(context);
        }

        context.Candidates = rerankResult.Value;
        return VKResult<VKSearchContext>.Success(context);
    }
}
```

---

## 2. ⚠️ 頻出アンチパターン

1. **RRF 計算内での非同期 I/O 呼び出し**:
   - ❌ `Compute()` 内で外部 API や DB を呼び出す。
   - ⭕ `VKReciprocalRankFusion` は純粋計算関数として保ち、I/O はパイプラインステージで事前完了させる。
2. **リランキング失敗時のパイプライン中断**:
   - ❌ Cross-Encoder のタイムアウトで検索リクエスト全体を HTTP 500 にする。
   - ⭕ 縮退挙動として粗召回（RRF 順位）の結果をそのまま返却する。

