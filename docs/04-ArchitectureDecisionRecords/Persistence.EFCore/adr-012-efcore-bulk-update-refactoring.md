# ADR 012: EF Core Bulk Optimization and Adapter Pattern for .NET 10

## 1. Meta Data

- **Date**: 2026-02-25
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: EF Core Repository Bulk Operations Refactoring (.NET 8.0 -> .NET 10.0)

## 2. Context (背景)

現在、システム全体を .NET 8.0 から .NET 10.0 へ移行する取り組みが進行中です。その一環として、データアクセスを担う `src\BuildingBlocks\Persistence\EFCore` モジュールの最適化が求められています。
.NET 8 時代の `ExecuteUpdateAsync` や `ExecuteDeleteAsync` などの Bulk 操作（一括処理）のインターフェース設計においては、EF Core のプロパティ更新を動的に表現するため、独自の Expression 生成ロジック（`EfCorePropertySetter`）を採用していました。また、処理結果の Logging には標準的な文字列補間（String Interpolation）を利用していました。
新しい .NET バージョンおよび EF Core の進化に伴い、API の呼び出し方針（Builder Pattern の推奨への移行）や、ゼロアロケーションを目指す Logging のベストプラクティス（Source Generators）が一般化してきているため、これらのアーキテクチャを見直す決定を行いました。

## 3. Problem Statement (問題定義)

現在の実装（.NET 8.0 向け）には、主に以下のアーキテクチャ上の課題が存在します。

1. **EF Core API の進化と OCP（開放閉鎖の原則）の懸念**:
   旧実装では、`IPropertySetter<TEntity>` の定義に対して、動的に `SetPropertyExpression`（Expression Tree）を組み立て、それを `ExecuteUpdateAsync` に渡していました。しかし、EF Core のバージョンが上がるにつれ、`UpdateSettersBuilder<T>` を活用した fluent なセットアップが主流となり、独自の Expression Tree 生成ロジックは破壊的変更の煽りを受けやすく、拡張性瓶頸（Extensibility Bottleneck）となっていました。

    _Smell_:

    ```csharp
    // .NET 8 Style
    var propertySetter = new EfCorePropertySetter<TEntity>();
    setPropertyAction(propertySetter);
    var setPropertyExpression = propertySetter.BuildSetPropertyExpression(); // 複雑な Expression 解析
    await DbSet.Where(predicate).ExecuteUpdateAsync(setPropertyExpression, cancellationToken);
    ```

2. **Performance (Logging のオーバーヘッド)**:
   Bulk 操作後のロギングにおいて、`typeof(TEntity).Name` を介したリフレクション呼び出しと、オブジェクトのボクシング（Object Boxing）が発生していました。高頻度で呼ばれる Repository の基盤コードにおいて、これは不要な GC プレッシャーを生み出します。

## 4. Decision (決定事項)

上記の課題を解決するため、以下のアーキテクチャ変更を採用します。

1. **Adapter Pattern による `UpdateSettersBuilder` の隠蔽化**:
   .NET 10 向けに、`IPropertySetter<TEntity>` の実装として `EfCorePropertySetterAdapter<TEntity>` を導入します。これにより、ドメイン層やアプリケーション層から EF Core 固有の `UpdateSettersBuilder` への依存を防ぎ（Clean Architecture を維持）、内部的には直接 EF Core の Builder に委譲します。

2. **High-Performance Logging の導入**:
   `[LoggerMessage]` 属性を活用した静的な Source Generated Logger (`EfCoreRepoLogger`) を実装します。また、`typeof(TEntity).Name` の代わりに静的型キャッシュ（`EfCoreTypeCache<TEntity>.EntityName`）を使用し、リフレクションを完全に排除します。

**設計詳細（Core Classes 草案）**:

```csharp
// 1. Adapter 導入による Clean Architecture の維持
internal sealed class EfCorePropertySetterAdapter<TEntity>(UpdateSettersBuilder<TEntity> builder)
    : IPropertySetter<TEntity> where TEntity : class
{
    public IPropertySetter<TEntity> SetProperty<TProperty>(
        Expression<Func<TEntity, TProperty>> propertyExpression,
        TProperty value)
    {
        builder.SetProperty(propertyExpression, value); // Native Builder に委譲
        return this;
    }
}

// 2. ExecuteUpdateAsync の変更 (.NET 10+)
public async Task<int> ExecuteUpdateAsync(
    Expression<Func<TEntity, bool>> predicate,
    Action<IPropertySetter<TEntity>> setPropertyAction,
    CancellationToken cancellationToken = default)
{
    var updatedRows = await DbSet.Where(predicate).ExecuteUpdateAsync(builder =>
    {
        var adapter = new EfCorePropertySetterAdapter<TEntity>(builder);
        setPropertyAction(adapter);
        _processor.ProcessBulkUpdate(adapter); // Auditing 処理も Adapter 経由で実行
    }, cancellationToken);

    // 3. 高速 Logging
    logger.LogBulkUpdateSuccess(updatedRows, EfCoreTypeCache<TEntity>.EntityName);
    return updatedRows;
}
```

## 5. Alternatives Considered (替代案の検討)

- **Option 1: `IPropertySetter` 抽象を廃止し、直接 `UpdateSettersBuilder` を公開する**
    - **Approach**: Application/Domain レイヤーのコードが直接 `Microsoft.EntityFrameworkCore.Query.UpdateSettersBuilder` に依存するようにインターフェースを変更する。
    - **Rejected Reason**: EF Core への強結合が発生し、将来的に他の ORM への移行や Unit Test の際に Mock が困難になるため、Clean Architecture に違反する（DIP 違反）。

- **Option 2: 既存の Expression Tree 生成モデルを継続維持する**
    - **Approach**: EF Core のバージョンが上がっても、自前で `Expression<Func<SetPropertyCalls...>>` を組み立てる実装をそのまま持ち越す。
    - **Rejected Reason**: EF Core 内部の API 変更に追従するメンテナンスコストが高く、Native の Fluent Builder（ラムダ式渡し）のパフォーマンス的な恩恵を享受できないため。

## 6. Consequences & Mitigation (結果と緩和策)

- **Positive**:
    - **Architecture**: `IPropertySetter<TEntity>` による抽象化が維持されるため、上位レイヤーでの利用コードに変更が生じず、後方互換性が保たれます。
    - **Performance**: Source Generators と Static Type Caching により、Bulk 操作実行時のアロケーションと CPU サイクルが劇的に削減されます。
- **Negative**:
    - `#if NET8_0` と `#else` による条件付きコンパイル指示子が存在し、Repository クラスの実装の可読性が若干低下（複雑性増加）します。
- **Mitigation**:
    - **対応策**: .NET 10.0 への完全な移行が終了し、.NET 8.0 への後方互換性の担保が不要になったタイミングで、古い `EfCorePropertySetter` と共に関連する `#if NET8_0` のブロックを削除し、コードをクリーンアップします。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- **データ構造・例外処理方針**:
    - `ArgumentNullException.ThrowIfNull(predicate)` および `setPropertyAction` の null チェックをフェイルファスト（Fail-fast）で行い、予期しない一括更新（全件更新等）を防ぎます。
    - Adapter 内での委譲呼び出しは、EF Core が内部で持つ Parameterization の仕組みをそのまま経由するため、SQL Injection に対する耐性を維持します。
- **セキュリティの重点**:
    - **Log Forging / 情報漏洩 防御**: `[LoggerMessage]` パターンを利用し、引数が厳密に型付けられるため、悪意のある入力がログのフォーマットを崩す攻撃（Log Injection）を極めて困難にします。
    - **監査証跡（Auditing）の確実性**: Bulk 操作は EF Core の ChangeTracker をバイパスするため（Validation はスキップされる）、Adapter を生成した直後に意図的に `_processor.ProcessBulkUpdate(adapter)` を呼び出し、アプリケーション共通の Audit 項目（更新日時・更新者など）が強制的に付与される安全な機構を引き続き担保しています。

## 8. Implementation References (参考リンク)

- 初期提案: `src\BuildingBlocks\Persistence\EFCore` にて .NET 8.0 -> .NET 10.0 の Bulk 操作リファクタリングを実施。
- ソースコード:
    - `src\BuildingBlocks\Persistence\EFCore\Repositories\EfCorePropertySetterAdapter.cs`
    - `src\BuildingBlocks\Persistence\EFCore\Repositories\EfCoreRepository.Bulk.cs`
    - `src\BuildingBlocks\Persistence\EFCore\Internal\EfCoreRepoLogger.cs`
- 設計意図: `ExecuteUpdateAsync` と `ExecuteDeleteAsync` メソッドを再構築し、抽象度を損なうことなく `EfCorePropertySetterAdapter` と `EfCoreRepoLogger` による高速化を実現。

# 補足（Pending Issues & Observations）

以下の2点の未解決課題・違和感は今後のタスクとして対処が必要です。

1. **`IUnitOfWork<TDbContext>` の抽象漏れ（Abstraction Leak）未解決**
    - 監査レポートにて「アプリケーション層に露出するインターフェース `IUnitOfWork` にEFCore依存のGeneric型 `<TContext>` が漏出している」との指摘がありましたが、まだ修正が完了していません。

2. **`WarnIfInsecureCursor` メソッドの実装上の違和感**
    - 本番環境での非セキュアなカーソルシリアライザーの使用を警告する処理ですが、現在のDIの `Options` パイプライン内での検証アプローチに対して、ユーザーより懸念（ちょっと問題がある感覚）が示されています。設計の再考が必要です。
