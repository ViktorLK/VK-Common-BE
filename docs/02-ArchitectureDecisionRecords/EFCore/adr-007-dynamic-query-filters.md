# ADR 007: Dynamic Global Query Filters via Reflection

**Date**: 2026-02-15  
**Status**: ✅ Accepted  
**Deciders**: Architecture Team  
**Technical Story**: [EFCore Persistence Layer - Soft Delete Automation]

---

## Context (背景)

### Problem Statement (問題定義)

Soft Delete パターンを実装する際、すべての `ISoftDelete` エンティティに対して、**手動で Global Query Filter を設定**する必要がある：

```csharp
// ❌ 手動設定（保守性が低い）
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Product>().HasQueryFilter(e => !e.IsDeleted);
    modelBuilder.Entity<Order>().HasQueryFilter(e => !e.IsDeleted);
    modelBuilder.Entity<Customer>().HasQueryFilter(e => !e.IsDeleted);
    // ... 100個のエンティティで同じコードを繰り返す
}
```

**問題点**:

1. **DRY 違反**: 同じコードを繰り返し記述
2. **設定漏れのリスク**: 新しいエンティティを追加した際、フィルタ設定を忘れる可能性
3. **保守性の低下**: `ISoftDelete` の実装を変更した場合、すべての箇所を修正

### Business Impact (ビジネスへの影響)

**設定漏れの例**:

```csharp
// フィルタ設定を忘れた場合
var products = await context.Products.ToListAsync();
// → 削除済み（IsDeleted = true）のデータも含まれる！
// → データ漏洩のリスク
```

---

## Decision (決定事項)

我々は **Reflection + Delegate による動的な Global Query Filter の自動適用** を採用する。

### Core Strategy (コア戦略)

```csharp
// BaseDbContextExtensions.cs
public static void ApplyGlobalFilters(this ModelBuilder modelBuilder)
{
    foreach (var entityType in modelBuilder.Model.GetEntityTypes())
    {
        // 抽象クラスやインターフェースはスキップ
        if (entityType.ClrType.IsAbstract || entityType.ClrType.IsInterface)
            continue;

        // 継承階層で既にフィルタが設定されている場合はスキップ
        if (entityType.BaseType is not null &&
            typeof(ISoftDelete).IsAssignableFrom(entityType.BaseType.ClrType))
            continue;

        // ISoftDelete を実装している場合、フィルタを適用
        if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
        {
            var setFilter = _filterSetters.GetOrAdd(entityType.ClrType, type =>
            {
                var concreteMethod = _setSoftDeleteFilterMethod.MakeGenericMethod(type);
                return (Action<ModelBuilder>)Delegate.CreateDelegate(
                    typeof(Action<ModelBuilder>),
                    concreteMethod
                );
            });

            setFilter(modelBuilder);
        }
    }
}

private static void SetSoftDeleteFilter<TEntity>(ModelBuilder modelBuilder)
    where TEntity : class, ISoftDelete
{
    modelBuilder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
}
```

### How It Works (動作原理)

#### 1. Reflection による MethodInfo の取得

```csharp
private static readonly MethodInfo _setSoftDeleteFilterMethod = typeof(BaseDbContextExtensions)
    .GetMethod(nameof(SetSoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)
    ?? throw new InvalidOperationException($"NotFound {nameof(SetSoftDeleteFilter)}");
```

#### 2. MakeGenericMethod による型パラメータの指定

```csharp
// Product 型の場合
var concreteMethod = _setSoftDeleteFilterMethod.MakeGenericMethod(typeof(Product));
// → SetSoftDeleteFilter<Product>(ModelBuilder) が生成される
```

#### 3. Delegate.CreateDelegate によるデリゲート生成

```csharp
var setFilter = (Action<ModelBuilder>)Delegate.CreateDelegate(
    typeof(Action<ModelBuilder>),
    concreteMethod
);

// 実行
setFilter(modelBuilder);
// → SetSoftDeleteFilter<Product>(modelBuilder) が呼ばれる
```

#### 4. ConcurrentDictionary によるキャッシュ

```csharp
private static readonly ConcurrentDictionary<Type, Action<ModelBuilder>> _filterSetters = new();

var setFilter = _filterSetters.GetOrAdd(entityType.ClrType, type =>
{
    // 初回のみリフレクション実行
    var concreteMethod = _setSoftDeleteFilterMethod.MakeGenericMethod(type);
    return (Action<ModelBuilder>)Delegate.CreateDelegate(...);
});
```

---

## Alternatives Considered (検討した代替案)

### ❌ Option 1: Manual Configuration (手動設定)

**Approach**: すべてのエンティティで手動設定。

```csharp
modelBuilder.Entity<Product>().HasQueryFilter(e => !e.IsDeleted);
modelBuilder.Entity<Order>().HasQueryFilter(e => !e.IsDeleted);
// ...
```

**Rejected Reason**:

- **DRY 違反**: コードの重複
- **設定漏れのリスク**: 新しいエンティティを追加した際に忘れる可能性

### ❌ Option 2: Convention-Based Configuration

**Approach**: EF Core の Convention を使用。

```csharp
public class SoftDeleteConvention : IEntityTypeAddedConvention
{
    public void ProcessEntityTypeAdded(...)
    {
        if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
        {
            // フィルタを適用
        }
    }
}
```

**Rejected Reason**:

- **複雑性**: Convention API の学習コスト
- **デバッグ困難**: Convention の実行順序が不明確

### ✅ Option 3: Reflection + Delegate (採用案)

**Advantages**:

- ✅ **DRY 原則**: コードの重複を排除
- ✅ **設定漏れの防止**: 自動的にすべての `ISoftDelete` エンティティに適用
- ✅ **パフォーマンス**: キャッシュにより、リフレクションは型ごとに1回のみ
- ✅ **保守性**: `ISoftDelete` の実装を変更しても、1箇所の修正で済む

---

## Consequences (結果)

### Positive (ポジティブな影響)

✅ **コード量削減**: 100行 → **10行**（90% 削減）  
✅ **設定漏れの防止**: 新しいエンティティを追加しても、自動的にフィルタが適用  
✅ **保守性向上**: フィルタロジックが1箇所に集約  
✅ **データ漏洩リスクの排除**: 削除済みデータが誤って表示されることを防止

### Negative (ネガティブな影響)

⚠️ **リフレクションのコスト**: `OnModelCreating` 実行時に若干のオーバーヘッド（ただし、アプリ起動時に1回のみ）  
⚠️ **デバッグの複雑性**: リフレクションによる動的生成のため、ステップ実行が困難

### Mitigation (緩和策)

- 📊 **キャッシュ**: `ConcurrentDictionary` でデリゲートをキャッシュ
- 🧪 **統合テスト**: フィルタが正しく適用されることを検証
- 💬 **ログ出力**: フィルタ適用時にログを出力（デバッグ用）

---

## Performance Analysis (性能分析)

### Reflection Cost (リフレクションコスト)

| 操作                  | 実行時間                     | 頻度              |
| --------------------- | ---------------------------- | ----------------- |
| **GetMethod**         | ~1 ms                        | アプリ起動時に1回 |
| **MakeGenericMethod** | ~0.5 ms                      | 型ごとに1回       |
| **CreateDelegate**    | ~0.2 ms                      | 型ごとに1回       |
| **合計**              | ~1.7 ms × 100型 = **170 ms** | アプリ起動時のみ  |

**影響**:

- アプリ起動時に1回だけ実行されるため、実行時のパフォーマンスには影響なし
- キャッシュにより、2回目以降はリフレクション不要

---

## Implementation References (実装参照)

### Core Components (コアコンポーネント)

- [`BaseDbContextExtensions.cs`](/src/BuildingBlocks/Persistence/EFCore/Extensions/BaseDbContextExtensions.cs) - Global Query Filter の自動適用

### Usage in BaseDbContext (BaseDbContext での使用)

[`BaseDbContext.cs:33-39`](/src/BuildingBlocks/Persistence/EFCore/BaseDbContext.cs#L33-L39)

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.ApplyGlobalFilters();  // ← 自動適用
    modelBuilder.ApplyConcurrencyToken();
}
```

---

## Deep Dive: Reflection Techniques (Reflection 技術の詳細)

### 1. MethodInfo.MakeGenericMethod()

**非ジェネリックメソッド**:

```csharp
public static void PrintValue(object value)
{
    Console.WriteLine(value);
}
```

**ジェネリックメソッド**:

```csharp
public static void PrintValue<T>(T value)
{
    Console.WriteLine(value);
}

// 実行時に型パラメータを指定
var method = typeof(MyClass).GetMethod("PrintValue");
var genericMethod = method.MakeGenericMethod(typeof(int));
genericMethod.Invoke(null, new object[] { 123 });
// → PrintValue<int>(123) が実行される
```

### 2. Delegate.CreateDelegate()

**メソッドをデリゲートに変換**:

```csharp
// 静的メソッド
public static void MyMethod(string arg) { ... }

// デリゲートに変換
var method = typeof(MyClass).GetMethod("MyMethod");
var action = (Action<string>)Delegate.CreateDelegate(typeof(Action<string>), method);

// 実行
action("Hello");  // → MyMethod("Hello") が実行される
```

**メリット**:

- リフレクション呼び出し（`Invoke()`）より高速
- 型安全なデリゲートとして扱える

---

## Related Patterns (関連パターン)

### 1. Convention over Configuration (設定より規約)

`ISoftDelete` インターフェースを実装するだけで、自動的にフィルタが適用される：

- 明示的な設定不要
- 規約に従うだけで動作

### 2. Template Method Pattern (テンプレートメソッドパターン)

`SetSoftDeleteFilter<T>()` は、フィルタ適用の**テンプレート**：

- 型パラメータだけが異なる
- ロジックは共通

---

## Related Documents (関連ドキュメント)

- 📄 [ADR-001: Hybrid Auditing Strategy](./adr-001-hybrid-auditing.md) - Soft Delete の使用シナリオ
- 📄 [Architecture Audit Report](/docs/AuditReports/EFCore_Persistence_20260218.md) - Dynamic Query Filters の評価
- 📖 [Global Query Filters (EF Core)](https://learn.microsoft.com/en-us/ef/core/querying/filters)

---

## Future Considerations (将来的な検討事項)

### 1. Multi-Tenancy Support (マルチテナント対応)

```csharp
private static void SetMultiTenantFilter<TEntity>(ModelBuilder modelBuilder, string tenantId)
    where TEntity : class, IMultiTenant
{
    modelBuilder.Entity<TEntity>().HasQueryFilter(e => e.TenantId == tenantId);
}
```

### 2. Conditional Filters (条件付きフィルタ)

```csharp
// 管理者モードではフィルタを無効化
if (!isAdminMode)
{
    modelBuilder.ApplyGlobalFilters();
}
```
