# アーキテクチャ監査レポート — EFCore 永続化レイヤー

## 📊 審査概要 (Audit Summary)

| 項目             | 内容                                                                                                                                                                             |
| ---------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **評点**         | **88 / 100**                                                                                                                                                                     |
| **現在の層判定** | Infrastructure Layer — EF Core Persistence Building Block                                                                                                                        |
| **一言評価**     | 「教科書的な Clean Architecture 実装。ただし、`IUnitOfWork` の型パラメータ漏洩と `EfCoreRepository` のコンストラクタ引数の nullable 設計が、抽象化の純粋性を一段階下げている。」 |

### スコア内訳

| 審査軸                    | 評点  | 所見                                                                                                       |
| ------------------------- | ----- | ---------------------------------------------------------------------------------------------------------- |
| 設計原則 (SOLID/KISS/DRY) | 18/20 | 全体的に優秀。DIP・SRP は模範的                                                                            |
| 設計パターン              | 16/20 | Repository・UoW・Strategy・Interceptor を適切に適用                                                        |
| アーキテクチャ原則        | 18/20 | 関心の分離・カプセル化が徹底されている                                                                     |
| アーキテクチャスタイル    | 17/20 | Clean Architecture に高度に準拠                                                                            |
| アーキテクチャパターン    | 15/20 | CQS 分離は優秀。UoW の型パラメータに設計上の疑問あり                                                       |
| エンタープライズパターン  | 14/20 | 楽観的ロック・ソフトデリート・監査は実装済。分散トランザクション・べき等性は未対応（スコープ外として妥当） |

---

## 🚨 致命的アーキテクチャ臭 (Critical Architectural Smells)

### ❌ [DIP 違反の疑い] `IUnitOfWork<TDbContext>` — 抽象化への具象型の漏洩

**対象ファイル**: [`UnitOfWork.cs`](../../src/BuildingBlocks/Persistence/EFCore/UnitOfWork.cs) (L17–18)、[`ServiceCollectionExtensions.cs`](../../src/BuildingBlocks/Persistence/EFCore/DependencyInjection/ServiceCollectionExtensions.cs) (L76)

```csharp
// UnitOfWork.cs L17-18
public class UnitOfWork<TDbContext> : IUnitOfWork<TDbContext>
    where TDbContext : DbContext

// ServiceCollectionExtensions.cs L76
services.AddScoped<IUnitOfWork, UnitOfWork<TContext>>();
```

**問題の本質**: `IUnitOfWork<TDbContext>` というインターフェースが `TDbContext : DbContext` という EF Core 固有の型制約を持つことで、Application Layer がインターフェース越しに EF Core の存在を認識してしまう。Clean Architecture では Application Layer は永続化技術に一切依存してはならない。

**影響**: Application Layer のコードが `IUnitOfWork<AppDbContext>` のように具象型を参照する場合、EF Core への依存が上位層に滲み出る。

**推奨対策**:

```csharp
// 技術非依存の純粋なインターフェース
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<ITransaction> BeginTransactionAsync(...);
    IBaseRepository<TEntity> Repository<TEntity>() where TEntity : class;
    // ...
}
```

---

### ❌ [設計の曖昧さ] `EfCoreRepository<TEntity>` — nullable な依存性注入引数

**対象ファイル**: [`EfCoreRepository.cs`](../../src/BuildingBlocks/Persistence/EFCore/Repositories/EfCoreRepository.cs) (L22–26)

```csharp
public partial class EfCoreRepository<TEntity>(
    DbContext context,
    ILogger<EfCoreRepository<TEntity>> logger,
    IEntityLifecycleProcessor? processor = null,   // ← nullable
    ICursorSerializer? cursorSerializer = null)     // ← nullable
```

**問題の本質**: コンストラクタ引数を `nullable` にすることは、「この依存性は任意である」という設計意図を示す。しかし `ICursorSerializer` は `EfCoreReadRepository` の基底クラスコンストラクタで `ArgumentNullException.ThrowIfNull` が呼ばれており、実際には必須である。この矛盾は DI コンテナが正しく設定されていない場合に実行時エラーとして顕在化する。

**影響**: DI 設定ミスが型システムではなく実行時に発覚する。コンパイル時の安全性が失われる。

---

## ⚠️ コード品質・規約リスク (Code Quality Risks)

### ⚠️ [SRP 軽微違反] `ServiceCollectionExtensions.cs` — `Configure()` の二重呼び出し

**対象ファイル**: [`ServiceCollectionExtensions.cs`](../../src/BuildingBlocks/Persistence/EFCore/DependencyInjection/ServiceCollectionExtensions.cs) (L107, L118–121)

```csharp
services.Configure(configureOptions);          // L107 — 1回目

services.AddOptions<CursorSerializerOptions>() // L118
    .Configure(configureOptions)               // L119 — 2回目 (重複)
    .Validate(...)
    .ValidateOnStart();
```

`configureOptions` アクションが二重に登録されており、オプションが二度適用される。`AddOptions<T>().Configure()` のみを使用すれば十分。

---

### ⚠️ [マジックナンバー] `GetCursorPagedAsync` — ページサイズ検証の不統一

**対象ファイル**: [`EfCoreReadRepository.Query.cs`](../../src/BuildingBlocks/Persistence/EFCore/Repositories/EfCoreReadRepository.Query.cs) (L71–74)

```csharp
if (pageSize <= 0)
{
    throw new ArgumentException("pageSize must greater than 0", nameof(pageSize));
}
```

`GetPagedAsync` では `PaginationValidator.ValidateOffsetPagination(pageNumber, pageSize)` を呼び出して集中管理しているが、`GetCursorPagedAsync` ではインラインで検証している。DRY 原則の軽微な違反。また、エラーメッセージが `RepositoryConstants.ErrorMessages` に集約されておらず、文字列リテラルが直書きされている。

---

### ⚠️ [ドキュメント誤記] `UnitOfWork.cs` — XML コメントの参照先誤り

**対象ファイル**: [`UnitOfWork.cs`](../../src/BuildingBlocks/Persistence/EFCore/UnitOfWork.cs) (L32)

```csharp
/// Initializes a new instance of the <see cref="EfCoreReadRepository{TEntity}"/> class.
```

`UnitOfWork<TDbContext>` のコンストラクタコメントが `EfCoreReadRepository{TEntity}` を参照している。コピー＆ペーストによる誤記。

---

### ⚠️ [パフォーマンス潜在リスク] `GetPagedAsync` — 全件 COUNT クエリ

**対象ファイル**: [`EfCoreReadRepository.Query.cs`](../../src/BuildingBlocks/Persistence/EFCore/Repositories/EfCoreReadRepository.Query.cs) (L30)

```csharp
var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);
```

オフセットページネーションでは `COUNT(*)` が常に実行される。大規模テーブルでは深刻なパフォーマンス問題になり得る。現時点では設計上の選択として許容できるが、将来的には `COUNT` をオプション化するか、推定件数 API の提供を検討すべき。

---

### ⚠️ [TODO 未解決] `UnitOfWork.cs` — 分離レベルサポートの未実装

**対象ファイル**: [`UnitOfWork.cs`](../../src/BuildingBlocks/Persistence/EFCore/UnitOfWork.cs) (L69)

```csharp
// TODO: Implement isolation level support
```

`BeginTransactionAsync` に `IsolationLevel` パラメータが存在するが、実際には `_context.Database.BeginTransactionAsync(isolationLevel, ...)` に渡されているため、EF Core 側では既にサポートされている。このコメントは誤解を招く可能性があり、削除または更新が必要。

---

### ⚠️ [セキュリティ注意] `SimpleCursorSerializer` — 本番環境での誤用リスク

**対象ファイル**: [`SimpleCursorSerializer.cs`](../../src/BuildingBlocks/Persistence/EFCore/Infrastructure/SimpleCursorSerializer.cs) (L12–14)

XML コメントで「本番環境では使用しないこと」と明記されているが、DI 登録では `SimpleCursorSerializer` がデフォルトとして登録される。開発者が `AddSecureCursorSerializer()` を呼び忘れた場合、本番環境で無署名のカーソルが使用されるリスクがある。環境変数や設定値による自動検出・警告ログの出力を検討すべき。

---

### ⚠️ [型安全性] `EfCoreExpressionCache` — `public` フィールドの露出

**対象ファイル**: [`EfCoreExpressionCache.cs`](../../src/BuildingBlocks/Persistence/EFCore/Caches/EfCoreExpressionCache.cs) (L17)

```csharp
public static readonly ConcurrentDictionary<...> _compiledExpressions = new(...);
```

`internal static` クラスにもかかわらず、フィールドが `public` で命名規則も `_` プレフィックス付き（privateフィールド慣習）。`private` または `internal` に変更し、`CachedCount` プロパティ経由でのみ外部公開すべき。

---

## ✅ 亮点 (Highlights)

### 🌟 模範的な CQS (Command Query Separation) の実装

`EfCoreReadRepository<TEntity>` (読み取り専用) と `EfCoreRepository<TEntity>` (読み書き両用) を明確に分離し、`partial class` で機能ごとにファイルを分割している。これは CQS 原則の教科書的な実装であり、テスタビリティと保守性を大幅に向上させる。

### 🌟 インターセプターによる横断的関心事の分離

`AuditingInterceptor` と `SoftDeleteInterceptor` を EF Core の `SaveChangesInterceptor` として実装し、監査フィールドの自動更新とソフトデリートのロジックをエンティティから完全に分離している。さらに、`eventData.Context is null` の場合に即座に例外をスローする防御的プログラミングが徹底されており、データ損失を防ぐ設計が秀逸。

### 🌟 バルク操作における監査の一貫性保証

EF Core の `ExecuteUpdate`/`ExecuteDelete` は ChangeTracker をバイパスするため、通常はインターセプターが機能しない。`EfCoreRepository.Bulk.cs` では `IEntityLifecycleProcessor.ProcessBulkUpdate/ProcessBulkSoftDelete` を明示的に呼び出すことで、バルク操作でも監査フィールドの一貫性を保証している。この設計判断は非常に重要であり、多くの実装で見落とされがちな落とし穴を回避している。

### 🌟 型安全なリフレクションキャッシュ

`EfCoreMethodInfoCache<TEntity>` では、`MethodInfo` をリフレクションで取得する代わりに、コンパイル時に型チェック可能な Expression Tree を使って `SetProperty` メソッドを検出している。これにより、EF Core のバージョンアップで API が変更された場合でもコンパイルエラーとして検出できる。

### 🌟 セキュアなカーソルシリアライザー

`SecureCursorSerializer` は HMAC-SHA256 署名、スキーマバージョニング、オプションの有効期限を備えた本格的な実装。特に `CryptographicOperations.FixedTimeEquals` によるタイミング攻撃対策は、セキュリティ意識の高さを示している。

### 🌟 グローバルクエリフィルターの自動適用

`BaseDbContextExtensions.ApplyGlobalFilters()` では、`ISoftDelete` を実装するすべてのエンティティに対して `HasQueryFilter(e => !e.IsDeleted)` を自動適用している。継承階層の重複フィルター適用を防ぐロジック (L37–40) も含まれており、細部まで配慮が行き届いている。

### 🌟 `IAsyncEnumerable<T>` によるストリーミング対応

`StreamAsync` メソッドで `IAsyncEnumerable<TEntity>` を返すことで、大量データの処理をメモリ効率よく行える設計を提供している。`[EnumeratorCancellation]` 属性の正しい使用も評価できる。

### 🌟 `WhereIf` / `OrderByIf` による宣言的クエリ構築

`QueryableExtensions` の条件付き LINQ 拡張メソッドにより、null チェックの if 文を排除し、クエリ構築を宣言的かつ読みやすく保っている。

---

## 💡 演進ロードマップ (Evolutionary Roadmap)

### 1. 立即修正 (Immediate Fix)

#### 1-1. `IUnitOfWork` インターフェースから型パラメータを除去する

Application Layer が EF Core に依存しないよう、`IUnitOfWork<TDbContext>` を `IUnitOfWork` に変更し、DI 登録側で具象型を解決する設計に移行する。

```csharp
// Before
services.AddScoped<IUnitOfWork, UnitOfWork<TContext>>();

// After — Application Layer は IUnitOfWork のみを参照
public interface IUnitOfWork { ... } // TDbContext 制約なし
```

#### 1-2. `ServiceCollectionExtensions.AddSecureCursorSerializer` の `Configure()` 二重呼び出しを修正

```csharp
// Before
services.Configure(configureOptions);           // 削除
services.AddOptions<CursorSerializerOptions>()
    .Configure(configureOptions)                // これだけで十分
    .Validate(...)
    .ValidateOnStart();
```

#### 1-3. `UnitOfWork.cs` の XML コメント誤記を修正

```csharp
// Before
/// Initializes a new instance of the <see cref="EfCoreReadRepository{TEntity}"/> class.

// After
/// Initializes a new instance of the <see cref="UnitOfWork{TDbContext}"/> class.
```

---

### 2. 推奨リファクタリング (Refactor)

#### 2-1. `GetCursorPagedAsync` のバリデーションを `PaginationValidator` に集約

```csharp
// Before (インライン)
if (pageSize <= 0)
    throw new ArgumentException("pageSize must greater than 0", nameof(pageSize));

// After (集中管理)
PaginationValidator.ValidateCursorPagination(pageSize);
```

#### 2-2. `EfCoreRepository<TEntity>` のコンストラクタ引数の null 許容性を整理

`ICursorSerializer` は基底クラスで必須のため、`EfCoreRepository` でも必須引数として宣言する。DI コンテナが常に提供することを前提とし、null 許容を廃止する。

#### 2-3. `SimpleCursorSerializer` の本番環境誤用防止

`AddVKDbContext` 内で、環境が `Production` の場合に `SimpleCursorSerializer` が登録されていれば警告ログを出力する仕組みを追加する。

```csharp
// 例: 起動時警告
if (env.IsProduction() && /* SimpleCursorSerializer が登録されている */)
{
    logger.LogWarning("SimpleCursorSerializer is registered in Production. " +
                      "Call AddSecureCursorSerializer() to enable HMAC protection.");
}
```

#### 2-4. `EfCoreExpressionCache._compiledExpressions` のアクセス修飾子修正

```csharp
// Before
public static readonly ConcurrentDictionary<...> _compiledExpressions = ...;

// After
private static readonly ConcurrentDictionary<...> _compiledExpressions = ...;
```

---

### 3. 学習推奨 (Learning)

#### 3-1. **Options パターンの深化** — `IOptionsSnapshot<T>` vs `IOptions<T>` vs `IOptionsMonitor<T>`

`CursorSerializerOptions` は `IOptions<T>` (Singleton スコープ) で注入されているが、将来的にホットリロードが必要になった場合は `IOptionsMonitor<T>` への移行を検討する。各インターフェースのライフタイムと用途の違いを理解することで、設定管理の設計が向上する。

> 参考: [Microsoft Docs — Options pattern in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/options)

#### 3-2. **Specification パターン** — クエリロジックのさらなる分離

現在、`predicate` は `Expression<Func<TEntity, bool>>` として Repository メソッドに直接渡されている。Specification パターンを導入することで、複雑なクエリロジックをエンティティに近い場所でカプセル化し、テスタビリティをさらに高めることができる。

```csharp
public interface ISpecification<TEntity>
{
    Expression<Func<TEntity, bool>> Criteria { get; }
    List<Expression<Func<TEntity, object>>> Includes { get; }
}
```

#### 3-3. **EF Core Interceptor の高度な活用** — `IDbCommandInterceptor` によるクエリログ・スロークエリ検出

現在の `AuditingInterceptor` / `SoftDeleteInterceptor` は `SaveChangesInterceptor` を使用している。`IDbCommandInterceptor` を追加することで、実行されたSQLのスロークエリ検出やクエリログの構造化出力が可能になり、可観測性が大幅に向上する。

---

## 付録: ファイル別評価マトリクス

| ファイル                         | 評価       | 主な所見                                                       |
| -------------------------------- | ---------- | -------------------------------------------------------------- |
| `UnitOfWork.cs`                  | ⭐⭐⭐⭐   | 型パラメータ漏洩・コメント誤記を除けば優秀                     |
| `BaseDbContext.cs`               | ⭐⭐⭐⭐⭐ | 最小限かつ適切な基底クラス設計                                 |
| `EfCoreReadRepository.cs`        | ⭐⭐⭐⭐⭐ | CQS の模範実装                                                 |
| `EfCoreReadRepository.Query.cs`  | ⭐⭐⭐⭐   | カーソルページネーション実装は高品質。バリデーション不統一あり |
| `EfCoreRepository.cs`            | ⭐⭐⭐⭐   | nullable 引数の設計に疑問あり                                  |
| `EfCoreRepository.Bulk.cs`       | ⭐⭐⭐⭐⭐ | バルク操作の監査一貫性保証が秀逸                               |
| `EfCorePropertySetter.cs`        | ⭐⭐⭐⭐⭐ | Expression Tree の巧みな活用                                   |
| `AuditingInterceptor.cs`         | ⭐⭐⭐⭐⭐ | 防御的プログラミングの模範                                     |
| `SoftDeleteInterceptor.cs`       | ⭐⭐⭐⭐⭐ | データ損失防止のコメントが詳細で優秀                           |
| `EfCoreExpressionCache.cs`       | ⭐⭐⭐⭐   | アクセス修飾子の不整合あり                                     |
| `EfCoreMethodInfoCache.cs`       | ⭐⭐⭐⭐⭐ | 型安全なリフレクション回避の好例                               |
| `EfCoreTypeCache.cs`             | ⭐⭐⭐⭐⭐ | シンプルかつ効果的                                             |
| `EntityLifecycleProcessor.cs`    | ⭐⭐⭐⭐⭐ | SRP を完全遵守した実装                                         |
| `IEntityLifecycleProcessor.cs`   | ⭐⭐⭐⭐⭐ | 適切な抽象化                                                   |
| `ServiceCollectionExtensions.cs` | ⭐⭐⭐⭐   | Configure() 二重呼び出しを除けば優秀                           |
| `QueryableExtensions.cs`         | ⭐⭐⭐⭐⭐ | 宣言的クエリ構築の好例                                         |
| `BaseDbContextExtensions.cs`     | ⭐⭐⭐⭐⭐ | 継承階層の重複フィルター防止が秀逸                             |
| `SecureCursorSerializer.cs`      | ⭐⭐⭐⭐⭐ | 本番グレードのセキュリティ実装                                 |
| `SimpleCursorSerializer.cs`      | ⭐⭐⭐⭐   | 本番誤用リスクの軽減策が必要                                   |
| `CursorSerializerOptions.cs`     | ⭐⭐⭐⭐⭐ | Options パターンの正しい実装                                   |
| `RepositoryConstants.cs`         | ⭐⭐⭐⭐   | 文字列定数の集中管理は良い。一部未使用の可能性あり             |
