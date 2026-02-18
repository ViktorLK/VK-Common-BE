# 📊 EFCore プロジェクトアーキテクチャ監査報告書

## 総合スコア: **72/100**

**評価理由**:
EFCore プロジェクトは、堅実なエンタープライズグレードの永続化層アーキテクチャを示しており、Repository パターン、Unit of Work、Interceptor パターンなどのコアパターンを成功裏に適用しています。コード構造は明確で、関心事の分離が良好であり、論理削除や監査などのエンタープライズ機能も備えています。しかし、DI 設定における重大な欠陥（运行时リスク）、抽象化層への依存の漏れ、および一部の設計決定の一貫性の問題が存在し、これが保守性と拡張性を制限しています。

---

## ✅ アーキテクチャの強み (Architectural Strengths)

### 1. **優れた Repository パターンの実装**

- **準拠原則**: インターフェース分離の原則 (ISP)、依存性逆転の原則 (DIP)
- **ビジネス価値**:
    - `IBaseRepository<TEntity>` は ORM に依存しない抽象化層を提供し、上位のビジネスロジックを EF Core から切り離します。これにより、将来的な技術変更のリスクを軽減します。
    - `IEfCoreRepository<TEntity>` は、EF Core 固有の機能（[`ExecuteUpdateAsync`](/src/BuildingBlocks/Persistence/EFCore/EfCoreRepository.cs#L244-L284) / [`ExecuteDeleteAsync`](/src/BuildingBlocks/Persistence/EFCore/EfCoreRepository.cs#L286-L303)）を拡張しており、修正ではなく拡張の原則に準拠しています。これにより、既存のコードを壊さずに新機能を追加できます。
    - クエリメソッドの命名が明確（[`GetFirstOrDefaultAsync`](/src/BuildingBlocks/Persistence/EFCore/EfCoreRepository.cs#L26-L31) vs [`GetSingleOrDefaultAsync`](/src/BuildingBlocks/Persistence/EFCore/EfCoreRepository.cs#L40-L45)）であり、開発者の意図を明確に伝えます。

### 2. **Unit of Work パターンの適切な適用**

- **準拠原則**: トランザクションスクリプトパターン、ACID 原則
- **ビジネス価値**:
    - [`UnitOfWork`](/src/BuildingBlocks/Persistence/EFCore/UnitOfWork.cs) は、トランザクション境界管理（[`BeginTransactionAsync`](/src/BuildingBlocks/Persistence/EFCore/UnitOfWork.cs#L36-L46), [`CommitTransactionAsync`](/src/BuildingBlocks/Persistence/EFCore/UnitOfWork.cs#L48-L69), [`RollbackTransactionAsync`](/src/BuildingBlocks/Persistence/EFCore/UnitOfWork.cs#L71-L87)）をカプセル化し、データの整合性を保証します。
    - `Repository<TEntity>()` ファクトリメソッドはディクショナリキャッシュを使用しており、リポジトリインスタンスの重複作成を回避し、パフォーマンスを最適化しています。
    - 完全な Dispose パターン（`IDisposable` + `IAsyncDisposable`）を実装しており、リソース管理が標準化されています。

### 3. **Interceptor パターンによるエンタープライズ機能の実装**

- **準拠原則**: 横断的関心事、アスペクト指向プログラミング (AOP)
- **ビジネス価値**:
    - [`AuditingInterceptor`](/src/BuildingBlocks/Persistence/EFCore/Interceptors/AuditingInterceptor.cs) と [`SoftDeleteInterceptor`](/src/BuildingBlocks/Persistence/EFCore/Interceptors/SoftDeleteInterceptor.cs) により、EF Core の SaveChanges インターセプターを介して監査と論理削除を自動処理します。これにより、開発者が手動でこれらの処理を記述する必要がなくなり、実装漏れを防ぎます。
    - 横断的関心事とビジネスロジックが完全に分離されており、単一責任の原則 (SRP) に準拠しています。
    - `ChangeTracker.Entries<T>()` ジェネリックトラバーサルを使用しており、型安全かつ効率的です。

### 4. **ページネーション戦略の完全性**

- **準拠原則**: ページネーションのベストプラクティス
- **ビジネス価値**:
    - Offset Pagination ([`GetPagedAsync`](/src/BuildingBlocks/Persistence/EFCore/EfCoreRepository.cs#L153-L185)) と Cursor Pagination ([`GetCursorPagedAsync`](/src/BuildingBlocks/Persistence/EFCore/EfCoreRepository.cs#L187-L221)) の両方の戦略を提供し、多様なユースケースに対応します。
    - Cursor Pagination は大規模データセットのシナリオに適しており、深いページのパフォーマンス問題を回避します。これにより、データ量が増加してもユーザー体験を損ないません。
    - `PaginationValidator` はページネーションパラメータを一元的に検証し、DRY 原則に準拠しています。

### 5. **動的な式ツリー操作**

- **準拠原則**: メタプログラミングパターン
- **ビジネス価値**:
    - [`ExecuteUpdateAsync`](/src/BuildingBlocks/Persistence/EFCore/EfCoreRepository.cs#L244-L284) では、Expression Trees を使用して監査フィールド（`UpdatedAt`, `UpdatedBy`）を動的に追加し、ハードコーディングを回避しています。これにより、柔軟で保守性の高いコードを実現しています。
    - [`ApplySoftDeleteProperties`](/src/BuildingBlocks/Persistence/EFCore/EfCoreRepository.cs#L304-L318) メソッドは、インターフェース検出と動的プロパティ設定を組み合わせ、高度なジェネリックプログラミング能力を示しています。

### 6. **構成駆動の拡張性**

- **準拠原則**: 開放/閉鎖原則 (OCP)
- **ビジネス価値**:
    - [`PersistenceOptions`](/src/BuildingBlocks/Persistence/Abstractions/Options/PersistenceOptions.cs) は、構成駆動の機能トグル（`EnableAuditing`, `EnableSoftDelete`）を提供します。これにより、コードを変更することなく、環境ごとに機能をオン/オフできます。
    - `ServiceCollectionExtensions.AddVKDbContext` はデリゲートを通じて DbContext 構成を注入し、柔軟性を高めています。

---

## ⚠️ アーキテクチャ上のリスク (Architectural Risks & Smells)

### 1. **重大: UnitOfWork DI 構成エラー (ランタイムクラッシュのリスク)** 🔴

- **違反**: 依存性注入のベストプラクティス
- **問題コード**:
    ```csharp
    // ServiceCollectionExtensions.cs, Line 57
    services.AddScoped<IUnitOfWork, UnitOfWork>();
    ```
- **原因**: [`UnitOfWork`](/src/BuildingBlocks/Persistence/EFCore/UnitOfWork.cs) のコンストラクタは [`DbContext`](/src/BuildingBlocks/Persistence/EFCore/BaseDbContext.cs) を必要としますが、`AddDbContext<TContext>` は **`TContext`（例: `MyDbContext`）のみを登録**し、基底クラスの [`DbContext`](/src/BuildingBlocks/Persistence/EFCore/BaseDbContext.cs) は自動登録されません。
- **結果**: **実行時に `InvalidOperationException: Unable to resolve service for type 'DbContext'` がスローされ、アプリケーションがクラッシュします。**
- **深刻度**: **Critical** - これはブロッキング欠陥であり、実行時エラーを 100% 引き起こします。

### 2. **中等: EF Core の抽象化層への漏洩** 🟡

- **違反**: 依存性逆転の原則 (DIP)、クリーンアーキテクチャ
- **問題コード**:
    ```csharp
    // IBaseRepository.cs
    Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null
    ```
- **原因**: `IQueryable<T>` は LINQ to Entities の具体的な実装であり、抽象インターフェースが ORM 固有の型を公開すべきではありません。
- **結果**:
    - 将来的に Dapper や MongoDB に切り替える場合、`include` パラメータは無意味になります。
    - 上位の呼び出し元が EF Core 固有のメソッド（例: `.Include(x => x.Orders).ThenInclude(...)`）を誤用する可能性があります。
- **提案**: 文字列配列 `string[] includeProperties` の使用や、ORM に依存しないロード戦略パターンの定義を検討してください。

### 3. **中等: Expression Tree 構築における潜在的なバグ** 🟡

- **違反**: 防御的プログラミング
- **問題コード**:
    ```csharp
    // EfCoreRepository.cs, Line 271-272
    Expression.Convert(Expression.Property(
        Expression.Convert(Expression.Parameter(typeof(TEntity), "e"), typeof(IAuditable)),
        "UpdatedAt"), typeof(DateTimeOffset?)),
    Expression.Parameter(typeof(TEntity), "e")  // ⚠️ 新しい Parameter を作成しています！
    ```
- **問題**: **`Expression.Parameter(typeof(TEntity), "e")` を呼び出すたびに新しい `ParameterExpression` インスタンスが作成されます**。これらのパラメータは、Expression Tree 内では異なる変数として扱われます。
- **結果**: EF Core の翻訳失敗や、誤った SQL 生成につながる可能性があります（現在は偶然動作しているかもしれませんが、論理的に正しくありません）。
- **正しいアプローチ**: 同じ `ParameterExpression` オブジェクトを再利用する必要があります。

### 4. **軽度: ハードコーディングされた日本語のエラーメッセージ** 🟡

- **違反**: 国際化 (I18N) のベストプラクティス
- **問題コード**:
    ```csharp
    // RepositoryConstants.cs
    public const string TransactionAlreadyActive = "既にアクティブなトランザクションが存在します";
    ```
- **問題**: 日本語のエラーメッセージがハードコーディングされており、多言語サポートが欠けています。
- **結果**: 国際展開時に大規模なリファクタリングが必要になります。

### 5. **設計の不一致: `PersistenceOptions.CreatedAtPropertyName` 未使用** 🟠

- **違反**: YAGNI (You Aren't Gonna Need It)
- **問題**:
    - [`PersistenceOptions`](/src/BuildingBlocks/Persistence/Abstractions/Options/PersistenceOptions.cs) で `CreatedAtPropertyName` を定義していますが、[`AuditingInterceptor`](/src/BuildingBlocks/Persistence/EFCore/Interceptors/AuditingInterceptor.cs) は [`IAuditable`](/src/BuildingBlocks/Persistence/Abstractions/Entities/IAuditable.cs) インターフェースを直接使用しています（属性名はインターフェースで `CreatedAt` に固定されています）。
    - この設定項目には現在、コンシューマーが存在しません。
- **結果**: 設定項目が「デッドコード」となり、開発者を混乱させます。

### 6. **リソースリークのリスク: Interceptor のライフサイクル管理** 🟠

- **違反**: リソース管理のベストプラクティス
- **問題**:
    ```csharp
    // ServiceCollectionExtensions.cs, Line 46
    var auditInterceptor = sp.GetRequiredService<AuditingInterceptor>();
    builder.AddInterceptors(auditInterceptor);
    ```
- **リスク**: `AddDbContext` の構成デリゲートは、[`DbContext`](/src/BuildingBlocks/Persistence/EFCore/BaseDbContext.cs) が作成されるたびに実行されますが、Interceptor は Scoped ライフサイクルです。[`DbContext`](/src/BuildingBlocks/Persistence/EFCore/BaseDbContext.cs) が複数回作成される場合（例: 同一スコープ内）、予期せぬ動作を引き起こす可能性があります。
- **提案**: [`DbContext`](/src/BuildingBlocks/Persistence/EFCore/BaseDbContext.cs) 構成における Scoped Interceptor の動作が期待通りか検証してください（通常は問題ありませんが、文書化が必要です）。

### 7. **例外処理とログの欠如** 🟡

- **違反**: Fail-Fast 原則、可観測性
- **問題**:
    - [`ExecuteUpdateAsync`](/src/BuildingBlocks/Persistence/EFCore/EfCoreRepository.cs#L244-L284) および [`ExecuteDeleteAsync`](/src/BuildingBlocks/Persistence/EFCore/EfCoreRepository.cs#L286-L303) に `DbUpdateException` の処理がありません。
    - 構造化ログ（例: `ILogger<T>`）がなく、一括操作の影響行数を追跡できません。
- **結果**: 本番環境での障害調査が困難になります。

---

## 💡 演進ロードマップ (Evolutionary Roadmap)

### 🔧 **即時修正 (Critical - P0)**

#### 1. UnitOfWork DI 構成の修正

```csharp
// ServiceCollectionExtensions.cs
public static IServiceCollection AddVKDbContext<TContext>(...)
{
    // ... existing code ...

    services.AddDbContext<TContext>((sp, builder) => { /* ... */ });

    // ✅ この行を追加し、DbContext を TContext に転送します
    services.AddScoped<DbContext>(sp => sp.GetRequiredService<TContext>());

    services.AddScoped<IUnitOfWork, UnitOfWork>();
    return services;
}
```

#### 2. Expression Tree パラメータ再利用の修正

```csharp
// EfCoreRepository.cs - ExecuteUpdateAsync
var entityParam = Expression.Parameter(typeof(TEntity), "e"); // ✅ 一回だけ作成

var updatedAtLambda = Expression.Lambda<Func<TEntity, DateTimeOffset?>>(
    Expression.Convert(
        Expression.Property(Expression.Convert(entityParam, typeof(IAuditable)), "UpdatedAt"),
        typeof(DateTimeOffset?)),
    entityParam); // ✅ 同じパラメータを再利用
```

---

### 🛠️ **高優先度 (High - P1)**

#### 3. IQueryable 依存の解耦

Specification パターンを導入し、`Func<IQueryable<T>, IQueryable<T>>` を代替します：

```csharp
public interface ISpecification<T>
{
    Expression<Func<T, bool>> Criteria { get; }
    List<string> Includes { get; }
}

Task<TEntity?> GetFirstOrDefaultAsync(
    ISpecification<TEntity> spec,
    CancellationToken cancellationToken = default);
```

#### 4. 未使用の設定項目の削除

```csharp
// PersistenceOptions.cs
public class PersistenceOptions
{
    public bool EnableAuditing { get; set; } = true;
    public bool EnableSoftDelete { get; set; } = true;
    // ❌ 削除: public string CreatedAtPropertyName { get; set; }
}
```

#### 5. 構造化ログの追加

```csharp
// EfCoreRepository.cs
public class EfCoreRepository<TEntity>(
    DbContext context,
    IAuditProvider auditProvider,
    ILogger<EfCoreRepository<TEntity>> logger) // ✅ ログ注入
{
    public async Task<int> ExecuteUpdateAsync(...)
    {
        var affectedRows = await query.ExecuteUpdateAsync(...);
        logger.LogInformation("Bulk update affected {Count} rows for {EntityType}",
            affectedRows, typeof(TEntity).Name);
        return affectedRows;
    }
}
```

---

### 📈 **中優先度 (Medium - P2)**

#### 6. エラーメッセージの国際化

リソースファイルまたは構成システムを使用します：

```csharp
// IStringLocalizer<T> の使用
public class UnitOfWork(DbContext context, IStringLocalizer<UnitOfWork> localizer)
{
    if (_currentTransaction is not null)
        throw new InvalidOperationException(localizer["TransactionAlreadyActive"]);
}
```

#### 7. 例外処理の強化

```csharp
public async Task<int> ExecuteUpdateAsync(...)
{
    try
    {
        return await query.ExecuteUpdateAsync(...);
    }
    catch (DbUpdateConcurrencyException ex)
    {
        logger.LogWarning(ex, "Concurrency conflict during bulk update");
        throw new DomainException("Update failed due to concurrent modification", ex);
    }
}
```

---

### 🔮 **長期的な最適化 (Low - P3)**

#### 8. ドメインイベントの導入

MediatR を統合し、`UnitOfWork.SaveChangesAsync` のタイミングでドメインイベントを発行します：

```csharp
public async Task<int> SaveChangesAsync(CancellationToken ct)
{
    await _mediator.DispatchDomainEventsAsync(_context, ct);
    return await _context.SaveChangesAsync(ct);
}
```

#### 9. パフォーマンスモニタリングの追加

Application Insights または Prometheus を統合します：

```csharp
using var activity = ActivitySource.StartActivity("EfCoreRepository.ExecuteUpdate");
activity?.SetTag("entity.type", typeof(TEntity).Name);
// ... execute query ...
activity?.SetTag("rows.affected", affectedRows);
```

#### 10. Read/Write リポジトリの分離 (CQRS)

クエリの複雑性が増した場合：

```csharp
public interface IReadRepository<TEntity> { /* 読み取り専用メソッド */ }
public interface IWriteRepository<TEntity> { /* 書き込みメソッド */ }
```

---

## 📝 まとめ

EFCore プロジェクトは、特にパターンの適用とエンタープライズ機能の面で強固なアーキテクチャ基盤を確立しています。**DI 構成の問題（Critical）は、本番環境でのクラッシュを防ぐために即座に修正する必要があります。** 中期的には、EF Core への依存を解消し、可観測性を強化することに注力すべきです。長期的には、複雑なビジネスシナリオに対応するために CQRS などの高度なパターンを検討できます。全体として、これは**アーキテクチャの方向性は正しいものの、微調整が必要なエンタープライズグレードの永続化層の実装**と言えます。
