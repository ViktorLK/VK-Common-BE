# 01. Core Standards (`CS.01`〜`CS.08`)

> **対応ルール**: [`.agents/rules/l3-vk-be-cs-core-standards.md`](/.agents/rules/l3-vk-be-cs-core-standards.md) (`CS.01`〜`CS.08`)

本ドキュメントは、`VK.Blocks` バックエンドにおける最も重要な **Type A 絶対制約（Result パターン、レイヤー依存、非同期、パフォーマンス、EF Core 自動化、決定論的抽象、DI 解決、DB 永続化標準）** の詳細解説と具体的な C# 実装例を提供します。

---

## `CS.01` — Result Pattern & Error Hierarchy 🔴
アプリケーション層および BuildingBlock のすべての操作は **`Result<T>`**（戻り値がない操作は `Result` または `Result<Unit>`）を返却します。

- **No Nulls & No Raw Strings**: `null` の返却や、`Result.Failure("raw string")` のような生文字列での失敗生成は厳禁です。
- **Exception Boundary**: インフラ層（DB・外部通信等）での例外発生は許容されますが、必ずアダプター境界でキャッチしてログ記録後、`Result<T>` にマッピングして返却します。HTTP 応答へは RFC 7807 `ProblemDetails` として変換します。
- **Error Constants Hierarchy**: エラーコードは `{ModuleName}.{Category}.{Reason}` の階層形式で `static readonly Error` 定数として定義します。
  - モジュール横断エラー: `VKCoreErrors`
  - 機能スライス固有エラー: `VK{FeatureName}Errors.cs`

```csharp
// 1. エラー定数の定義 (Contracts/VKApiKeyErrors.cs)
namespace VK.Blocks.Authentication;

public static class VKApiKeyErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Auth.ApiKey.NotFound",
        "The specified API key was not found.");

    public static readonly Error Expired = Error.Validation(
        "Auth.ApiKey.Expired",
        "The API key has already expired.");
}

// 2. ハンドラーでの使用例
public async Task<Result<Unit>> RevokeAsync(
    ApiKeyId id,
    CancellationToken ct = default)
{
    var entity = await _repository.FindByIdAsync(id, ct).ConfigureAwait(false);
    if (entity is null)
    {
        return Result.Failure<Unit>(VKApiKeyErrors.NotFound);
    }

    entity.Revoke(_timeProvider.GetUtcNow());
    await _repository.UpdateAsync(entity, ct).ConfigureAwait(false);

    return Result.Success(Unit.Value);
}
```

---

## `CS.02` — Layer Dependencies
- **依存方向の厳守**: Core / Application 層から具体的なインフラストラクチャライブラリ（EF Core, StackExchange.Redis, Azure SDK 等）への直接参照は禁止です。
- **抽象化**: DB・キャッシュ・メッセージング等のインフラ関心事はすべてインターフェース（`IVK...`）の背後に抽象化します。

---

## `CS.03` — Async & Synchronization Context 🔴
- **Async Everywhere**: すべての I/O 操作は `async/await` と `CancellationToken` を使用します。`.Result` や `.Wait()` による同期ブロッキングはデッドロックの原因となるため厳禁です。
- **`ConfigureAwait(false)` の使い分け**:
  - **BuildingBlock / ライブラリコード**: すべての `await` に **`.ConfigureAwait(false)` を必須** とします。
  - **単体テストコード (xUnit)**: テストランナーの同期コンテキストと並列制御を維持するため、**`.ConfigureAwait(false)` の使用を禁止** します（`xUnit1030` アナライザーで強制）。
- **`Task<T>` vs `ValueTask<T>`**:
  - **公開インターフェース (`IVK...`)**: 利用側の誤用（二重 await や同時アクセス）を防ぐため、必ず `Task<T>` を使用します。
  - **内部ホットパス (`internal`)**: キャッシュヒットなど同期完了が大半を占める内部メソッドに限り `ValueTask<T>` を優先します。

---

## `CS.04` — Query Performance & Zero-Allocation Buffers
- **Database Queries**:
  - ループ内での DB クエリ発行（N+1）は厳禁です。
  - すべての読み取りクエリは **`.AsNoTracking()` をデフォルト** とし、エンティティ全体の取得よりも `.Select(...)` による必要列の射影（Projection）を優先します。
  - 一括更新・削除には `ExecuteUpdateAsync` / `ExecuteDeleteAsync` を活用します。
  - ページネーション指定（`Skip`/`Take`）のない無制限の `ToListAsync()` は禁止です。
- **Memory & Buffers**:
  - 文字列解析にはヒープ割り当てを避けるため `ReadOnlySpan<char>` / `Span<T>` を使用します。
  - 256 bytes 以下の確定サイズの一時バッファには `stackalloc` を使用し、256 bytes を超えるバッファには `ArrayPool<T>.Shared` を使用して `finally` ブロックで確実に返却します。

```csharp
byte[] rented = ArrayPool<byte>.Shared.Rent(requiredLength);
try
{
    Span<byte> buffer = rented.AsSpan(0, requiredLength);
    // Process buffer without LOH/heap allocation
}
finally
{
    ArrayPool<byte>.Shared.Return(rented, clearArray: true);
}
```

---

## `CS.05` — Automated Auditing & Soft-Delete
- **`IAuditable` & `ISoftDelete`**: 監査フィールド（`CreatedAt`, `UpdatedAt`, `CreatedBy`）および論理削除（`IsDeleted`）は、すべて **EF Core DbContext Interceptors** と **Global Query Filters** によって自動処理します。
- アプリケーションコード内で手動で監査日時をセットしたり、`IsDeleted = true` を直接操作したりすることは禁止です。

---

## `CS.06` — Core Deterministic Abstractions 🔴
BuildingBlock 内で非決定論的なシステム API を直接呼び出すことは禁止です。必ず以下の抽象を DI 注入して使用します。

| 禁止される直接呼び出し | 注入して使用する抽象インターフェース |
| :--- | :--- |
| `Guid.NewGuid()` | `IVKGuidGenerator` (`Create()` で順次 GUID 生成) |
| `DateTime.UtcNow`, `DateTimeOffset.Now` | `TimeProvider` (`GetUtcNow()`) |
| `JsonSerializer.Serialize/Deserialize` | `IVKJsonSerializer` |

---

## `CS.07` — Deterministic Dependency Resolution 🔴
- **`GetRequiredService<T>()` Default**: `IServiceProvider` から登録済みであるべきサービスを解決する際は、必ず `GetRequiredService<T>()` を使用し、未登録時に即座に Fail-Fast させます。
- **`GetService<T>()` の禁止**: `null` を返してエラーを隠蔽する `GetService<T>()` は必須依存に対して使用禁止です。真にオプショナルな依存である場合のみ、明示的なフォールバック（`sp.GetService<T>() ?? DefaultInstance`）と理由コメントを伴って許容されます。

---

## `CS.08` — Persistence & Database Standards 🔴
EF Core エンティティ構成（`IEntityTypeConfiguration<T>`）および DB スキーマ設計では以下の制約を厳守します：

1. **Tenant Isolation**:
   - `TenantId` カラムは **`NOT NULL`** 必須。
   - マルチテナント高頻度クエリのインデックスは `TenantId` を先頭列とします（`IX_{Table}_TenantId_{BizKey}`）。
   - ※例外：親エンティティのナビゲーションプロパティ経由でのみアクセスされる純粋な正規化子テーブル（複合主キーの中間テーブル等）は、親の Global Filter で保護されるため `IVKTenantScoped` から免除されます。
2. **Primary Key**: すべてのエンティティは明示的な主キー `Id` を持ち、B-Tree ページ分割を防ぐため Sequential GUID / UUIDv7 を使用します。
3. **Timestamps**: `CreatedAt` は `NOT NULL` (`DateTimeOffset`)、`UpdatedAt` は作成時 `NULL`（実更新時のみインターセプタが設定）。
4. **String Boundaries**: すべての文字列プロパティには **`.HasMaxLength(N)` を明示的に指定** します。無制限の `VARCHAR(MAX)` / `TEXT` は禁止です。
5. **Relationships & Cascades**: すべての外部キーには `.HasConstraintName("FK_{Source}_{Target}_{Prop}")` を明示し、削除動作は **`DeleteBehavior.Restrict` または `NoAction` を必須** とします（暗黙の `Cascade` 削除は禁止）。
6. **Index Definitions**: すべてのインデックスには `.HasDatabaseName("IX_{Table}_{Columns}")` を明示し、論理削除テーブルのユニークインデックスには `.HasFilter("IsDeleted = 0")` を付与します。
7. **Migrations**: EF Core マイグレーションは Application Host が排他的に所有し、本番スクリプトは `dotnet ef migrations script --idempotent` で生成します。

```csharp
public sealed class ApiKeyRecordConfiguration : IEntityTypeConfiguration<ApiKeyRecord>
{
    public void Configure(EntityTypeBuilder<ApiKeyRecord> builder)
    {
        builder.ToTable("ApiKeys");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId).IsRequired().HasMaxLength(64);
        builder.Property(x => x.KeyHash).IsRequired().HasMaxLength(128);

        builder.HasIndex(x => new { x.TenantId, x.KeyHash })
            .IsUnique()
            .HasDatabaseName("IX_ApiKeys_TenantId_KeyHash")
            .HasFilter("IsDeleted = 0");
    }
}
```

