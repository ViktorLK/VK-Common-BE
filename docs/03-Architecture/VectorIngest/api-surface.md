# API Surface & Compatibility Contract: VectorIngest Subsystem

- **Scope**: `VK.Blocks.VectorIngest`
- **Specification Type**: Level 1 Public Surface & SemVer Boundary (`ARCH-07`, `AP.03`, `FS-09`)
- **Status**: ✅ Active

---

## 1. 📜 Public API Surface (Level 1 公開契約)

### 1.1 中核インターフェース契約 (`Contracts/`)

```csharp
namespace VK.Blocks.VectorIngest;

public interface IVKDocumentParser
{
    IReadOnlyList<string> SupportedMimeTypes { get; }
    ValueTask<VKResult<VKParsedDocument>> ParseAsync(
        Stream documentStream,
        CancellationToken cancellationToken = default);
}

public interface IVKDocumentParserResolver
{
    VKResult<IVKDocumentParser> Resolve(string mimeTypeOrExtension);
}

public interface IVKChunker
{
    VKChunkerType Type { get; }
    ValueTask<VKResult<IReadOnlyList<VKChunk>>> ChunkAsync(
        string text,
        VKChunkingOptions options,
        CancellationToken cancellationToken = default);
}

public interface IVKEmbeddingsEngine
{
    ValueTask<VKResult<IReadOnlyList<ReadOnlyMemory<float>>>> GenerateEmbeddingsAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken = default);

    // デフォルトインターフェースメソッド (DIM) による拡張
    ValueTask<VKResult<IReadOnlyList<VKSparseVector>>> GenerateSparseEmbeddingsAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken = default)
        => ValueTask.FromResult(VKResult<IReadOnlyList<VKSparseVector>>.Failure(
            VKVecEngineErrors.ExternalFailure.WithDescription("Sparse embedding not supported.")));

    ValueTask<VKResult<IReadOnlyList<VKMultiVector>>> GenerateMultiVectorEmbeddingsAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken = default)
        => ValueTask.FromResult(VKResult<IReadOnlyList<VKMultiVector>>.Failure(
            VKVecEngineErrors.MultiVectorNotSupported));
}
```

### 1.2 ドメイン列挙型とモデル (`Models/`)

```csharp
namespace VK.Blocks.VectorIngest;

public enum VKChunkerType : byte
{
    FixedSize = 0,
    Recursive = 1,
    Hierarchical = 2,
    Semantic = 3
}

public sealed record VKIndexingOptions : IVKBlockOptions
{
    public bool EnableSparseVector { get; init; } = false;
    public bool EnableMultiVector { get; init; } = false;
    public int MaxBatchSize { get; init; } = 100;
}
```

### 1.3 エラー定数 (`Errors/`)

```csharp
namespace VK.Blocks.VectorIngest;

public static class VKVectorIngestErrors
{
    public static readonly VKError UnsupportedMimeType;
    public static readonly VKError DocumentParsingFailed;
    public static readonly VKError ChunkingFailed;
    public static readonly VKError EmbeddingFailed;
    public static readonly VKError StorageWriteFailed;
}
```

