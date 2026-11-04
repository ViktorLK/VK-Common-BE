# Extension Guide: VectorStore Subsystem

- **Scope**: `VK.Blocks.VectorStore`
- **Specification Type**: Custom Provider & Strategy Implementation Guide (`ARCH-11`, `AP.06`)
- **Status**: ✅ Active

---

## 1. 🔌 新規 VectorStore プロバイダーの実装手順

`VK.Blocks.VectorStore` は、ポート＆アダプター分離（`ARCH-02`）に基づき、新しいベクトルデータベースや検索エンジンへの接続を容易に拡張できるアーキテクチャを採用しています。

### 1.1 プロバイダーが実装すべき契約

1. **バックエンド役割の特定 (`ADR-003`)**:
   - `DedicatedEngine`: コレクション作成・破棄の責務を持つため `IVKVectorCollectionLifecycle` を実装。
   - `RelationalColumn`: DDL は EF Core に委ね、`IVKVectorSchemaValidator` を実装。
   - `Embedded`: ローカルファイルまたはメモリ内で完結。
2. **能力の宣言 (`ADR-004`)**:
   - `VKVectorStoreCapabilities` フラグ（NativeHybrid, MultiVector, DiskIndex など）を正確に返す。
3. **コレクションアダプターの実装**:
   - `IVKVectorCollection<TRecord>` を実装し、すべての検索・更新処理で `VKGuard` による `TenantId` 検証を行う。

---

## 2. 📝 ゴールデンコード例 (最小限のカスタムプロバイダー)

```csharp
namespace VK.Blocks.VectorStore.Custom;

public sealed class CustomVectorStore : IVKVectorStore
{
    public VKVectorStoreBackendRole Role => VKVectorStoreBackendRole.DedicatedEngine;
    
    public VKVectorStoreCapabilities Capabilities => 
        VKVectorStoreCapabilities.NativeHybrid | VKVectorStoreCapabilities.FilterPushdown;

    public ValueTask<VKResult<IVKVectorCollection<TRecord>>> GetCollectionAsync<TRecord>(
        string collectionName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(collectionName))
        {
            return ValueTask.FromResult(VKResult<IVKVectorCollection<TRecord>>.Failure(
                VKVecEngineErrors.ExternalFailure.WithDescription("Collection name is required.")));
        }

        IVKVectorCollection<TRecord> collection = new CustomCollection<TRecord>(collectionName);
        return ValueTask.FromResult(VKResult<IVKVectorCollection<TRecord>>.Success(collection));
    }
}

internal sealed class CustomCollection<TRecord>(string collectionName) : IVKVectorCollection<TRecord>
{
    public string CollectionName { get; } = collectionName;

    public ValueTask<VKResult<bool>> UpsertBatchAsync(
        IEnumerable<TRecord> records,
        CancellationToken cancellationToken = default)
    {
        // 1. 引数防護 (AP.01)
        VKGuard.NotNull(records);

        try
        {
            // 2. 外部 SDK へのバッチ投入処理
            return ValueTask.FromResult(VKResult<bool>.Success(true));
        }
        catch (Exception ex)
        {
            // 3. 例外の確実な VKResult 変換 (CS.01)
            return ValueTask.FromResult(VKResult<bool>.Failure(
                VKVecEngineErrors.ExternalFailure.WithException(ex)));
        }
    }

    public async IAsyncEnumerable<VKVectorSearchResult<TRecord>> SearchStreamAsync(
        ReadOnlyMemory<float> vector,
        VKVectorSearchFilter? filter = null,
        int topK = 10,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // 4. 多租戸バリデーション (CS.08)
        if (filter?.TenantId == null)
        {
            yield return new VKVectorSearchResult<TRecord>(
                default!, 0f, VKVecEngineErrors.MissingTenantId);
            yield break;
        }

        // 5. 検索ストリーミング
        yield break;
    }

    public ValueTask<VKResult<TRecord?>> GetAsync(string key, CancellationToken cancellationToken = default)
        => ValueTask.FromResult(VKResult<TRecord?>.Success(default));

    public ValueTask<VKResult<bool>> DeleteAsync(string key, CancellationToken cancellationToken = default)
        => ValueTask.FromResult(VKResult<bool>.Success(true));
}
```

---

## 3. ⚠️ 頻出アンチパターンと回避策

1. **例外のスロー (Anti-Pattern)**:
   - ❌ `throw new RpcException(...)` や `throw new NpgsqlException(...)` をそのまま上位に投げる。
   - ⭕ `catch (Exception ex)` で捕捉し、`VKResult.Failure(VKVecEngineErrors.ExternalFailure.WithException(ex))` を返却する。
2. **多租戸フィルターの無視 (Anti-Pattern)**:
   - ❌ `filter` 引数を参照せず全件検索を発行する。
   - ⭕ 必ず `filter.TenantId` をエンジンの WHERE 句または Filter 辞書に結合する。
3. **リレーショナルストアでのコレクション作成 DDL (Anti-Pattern)**:
   - ❌ `Pgvector` プロバイダーで `CREATE TABLE` や `CREATE INDEX` を自動実行する。
   - ⭕ テーブル管理は EF Core Migration の主権とし、`IVKVectorSchemaValidator` で存在確認のみ行う。

