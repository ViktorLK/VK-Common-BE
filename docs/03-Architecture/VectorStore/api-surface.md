# API Surface & Compatibility Contract: VectorStore Subsystem

- **Scope**: `VK.Blocks.VectorStore`, `VK.Blocks.VectorStore.Pgvector`, `VK.Blocks.VectorStore.Qdrant`, `VK.Blocks.VectorStore.Cosmos`, `VK.Blocks.VectorStore.Sqlite`
- **Specification Type**: Level 1 Public Surface & SemVer Boundary (`ARCH-07`, `AP.03`, `FS-09`)
- **Status**: ✅ Active

---

## 1. 📜 Public API Surface (Level 1 公開契約)

以下のシンボルのみが、外部モジュールおよびアプリケーション層に対して意図的に公開されている Level 1 公開契約です。これら以外の具象実装（`Internal/` 配下）への直接アクセスは固く禁止されています。

### 1.1 中核インターフェース契約 (`Contracts/`)

```csharp
namespace VK.Blocks.VectorStore;

// ベクトルストア統合エントリーポイント
public interface IVKVectorStore
{
    VKVectorStoreBackendRole Role { get; }
    VKVectorStoreCapabilities Capabilities { get; }
    
    ValueTask<VKResult<IVKVectorCollection<TRecord>>> GetCollectionAsync<TRecord>(
        string collectionName,
        CancellationToken cancellationToken = default);
}

// コレクションレベル操作プロトコル
public interface IVKVectorCollection<TRecord>
{
    string CollectionName { get; }
    
    ValueTask<VKResult<bool>> UpsertBatchAsync(
        IEnumerable<TRecord> records,
        CancellationToken cancellationToken = default);

    ValueTask<VKResult<TRecord?>> GetAsync(
        string key,
        CancellationToken cancellationToken = default);

    ValueTask<VKResult<bool>> DeleteAsync(
        string key,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<VKVectorSearchResult<TRecord>> SearchStreamAsync(
        ReadOnlyMemory<float> vector,
        VKVectorSearchFilter? filter = null,
        int topK = 10,
        CancellationToken cancellationToken = default);
}

// 起動時スキーマ検証プロトコル (RelationalColumn 向け)
public interface IVKVectorSchemaValidator
{
    ValueTask<VKResult<bool>> ValidateSchemaAsync(
        string collectionName,
        CancellationToken cancellationToken = default);
}

// コレクション動的ライフサイクル管理 (DedicatedEngine 向け)
public interface IVKVectorCollectionLifecycle
{
    ValueTask<VKResult<bool>> CreateCollectionAsync(
        string collectionName,
        int dimensions,
        string distanceMetric = "cosine",
        CancellationToken cancellationToken = default);

    ValueTask<VKResult<bool>> DropCollectionAsync(
        string collectionName,
        CancellationToken cancellationToken = default);
}
```

### 1.2 ドメインモデル・列挙型 (`Models/`)

```csharp
namespace VK.Blocks.VectorStore;

// バックエンド役割区分 (ADR-003)
public enum VKVectorStoreBackendRole : byte
{
    DedicatedEngine = 0,
    RelationalColumn = 1,
    Embedded = 2
}

// エンジン能力フラグ (ADR-004)
[Flags]
public enum VKVectorStoreCapabilities : ushort
{
    None = 0,
    NativeHybrid = 1 << 0,
    NativeRRF = 1 << 1,
    MultiVector = 1 << 2,
    SparseVector = 1 << 3,
    DiskIndex = 1 << 4,
    FilterPushdown = 1 << 5
}

// スパースベクトル表現
public readonly record struct VKSparseVector(
    ReadOnlyMemory<int> Indices,
    ReadOnlyMemory<float> Values);

// 多ベクトル表現 (ColBERT / Token-level)
public readonly record struct VKMultiVector(
    IReadOnlyList<ReadOnlyMemory<float>> Vectors);
```

### 1.3 具象プロバイダー DI 拡張および構成オプション (`DependencyInjection/`)

```csharp
namespace VK.Blocks.VectorStore.Sqlite;

public static class VKVectorStoreSqliteServiceCollectionExtensions
{
    // SQLite 組込ベクトルストアの登録 (Role: Embedded)
    public static IServiceCollection AddVKVectorStoreSqlite(
        this IServiceCollection services,
        Action<VKSqliteVectorStoreOptions>? configure = null);
}

public sealed record VKSqliteVectorStoreOptions : IVKBlockOptions
{
    public string ConnectionString { get; init; } = "Data Source=vectors.db";
    public string? ExtensionPath { get; init; }
    public bool EnableWAL { get; init; } = true;
    public int BusyTimeoutMs { get; init; } = 5000;
}
```

```csharp
namespace VK.Blocks.VectorStore.Pgvector;

public static class VKVectorStorePgvectorServiceCollectionExtensions
{
    // PostgreSQL pgvector ストアの登録 (Role: RelationalColumn)
    public static IServiceCollection AddVKVectorStorePgvector(
        this IServiceCollection services,
        Action<VKPgvectorVectorStoreOptions>? configure = null);
}
```

```csharp
namespace VK.Blocks.VectorStore.Qdrant;

public static class VKVectorStoreQdrantServiceCollectionExtensions
{
    // Qdrant 専用ベクトルエンジンの登録 (Role: DedicatedEngine)
    public static IServiceCollection AddVKVectorStoreQdrant(
        this IServiceCollection services,
        Action<VKQdrantVectorStoreOptions>? configure = null);
}
```

### 1.4 エラー定数 (`Errors/`)

```csharp
namespace VK.Blocks.VectorStore;

public static class VKVecEngineErrors
{
    public static readonly VKError MissingTenantId;
    public static readonly VKError DimensionMismatch;
    public static readonly VKError LifecycleNotSupported;
    public static readonly VKError SchemaValidationFailed;
    public static readonly VKError MultiVectorNotSupported;
    public static readonly VKError ExternalFailure;
}
```

---

## 2. 🛡️ SemVer 後方互換性ポリシー (`FS-09`)

1. **破壊的変更（Major Version Up）**:
   - `IVKVectorStore` / `IVKVectorCollection<TRecord>` からのメソッド削除または引数シグネチャ変更。
   - `VKVecEngineErrors` に定義された固定エラーコード定数の名称変更・値変更。
   - `VKVectorStoreBackendRole` の既存メンバ値の再割り当て。
2. **後方互換な追加（Minor Version Up）**:
   - `VKVectorStoreCapabilities` への新規ビットフラグ追加。
   - 各種プロバイダー（`Sqlite`, `Pgvector`, `Qdrant`）向けの新規オプションプロパティ追加。
   - 新規エラー定数の追加。
