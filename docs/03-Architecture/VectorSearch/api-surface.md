# API Surface & Compatibility Contract: VectorSearch Subsystem

- **Scope**: `VK.Blocks.VectorSearch`, `VK.Blocks.VectorSearch.Sqlite`
- **Specification Type**: Level 1 Public Surface & SemVer Boundary (`ARCH-07`, `AP.03`, `FS-09`)
- **Status**: ✅ Active

---

## 1. 📜 Public API Surface (Level 1 公開契約)

### 1.1 純粋順位融合関数契約 (`Fusion/Contracts/`)

```csharp
namespace VK.Blocks.VectorSearch.Fusion.Contracts;

public static class VKReciprocalRankFusion
{
    // I/O 非依存の純粋数学計算関数
    public static IReadOnlyList<VKRankedItem> Compute(
        IReadOnlyList<IReadOnlyList<VKScoredItem>> rankedLists,
        int k = 60,
        IReadOnlyList<double>? weights = null);
}

public readonly record struct VKScoredItem(string Id, double Score);
public readonly record struct VKRankedItem(string Id, double Score, int Rank);
```

### 1.2 検索パイプライン契約 (`Pipeline/Contracts/`)

```csharp
namespace VK.Blocks.VectorSearch;

public interface IVKVectorSearchPipeline
{
    ValueTask<VKResult<IReadOnlyList<VKVectorSearchResultItem>>> ExecuteAsync(
        VKVectorSearchRequest request,
        CancellationToken cancellationToken = default);
}

public interface IVKSearchStage
{
    int Order { get; }
    ValueTask<VKResult<VKSearchContext>> ProcessAsync(
        VKSearchContext context,
        CancellationToken cancellationToken = default);
}

public interface IVKReranker
{
    ValueTask<VKResult<IReadOnlyList<VKVectorSearchResultItem>>> RerankAsync(
        string query,
        IReadOnlyList<VKVectorSearchResultItem> candidates,
        int topN,
        CancellationToken cancellationToken = default);
}
```

### 1.3 エラー定数 (`Errors/`)

```csharp
namespace VK.Blocks.VectorSearch;

public static class VKVectorSearchErrors
{
    public static readonly VKError PipelineExecutionFailed;
    public static readonly VKError StageTimeout;
    public static readonly VKError RerankFailed;
    public static readonly VKError EmptyQuery;
    public static readonly VKError StoreUnavailable;
}
```

