# ADR 011: Null Object Pattern for IEntityLifecycleProcessor

## 1. Meta Data

- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: EFCore Persistence Infrastructure Refinement & Dependency Inversion

## 2. Context (背景)

本決定は、EF Core パス（`src/BuildingBlocks/Persistence/EFCore`）におけるインフラストラクチャサービスの依存関係注入（DI）と、コンストラクタインジェクションにおける `Nullable` 制約の厳格化を背景としています。
以前の監査（`EFCore_Persistence_20260218.md`）において、「基礎となるリポジトリクラス（`EfCoreRepository`等）のコンストラクタ引数で `?` (Nullable) を用いるべきではなく、依存関係は明確に要求して Fail-Fast させるべきである」というアーキテクチャ上の指摘がありました。

## 3. Problem Statement (問題定義)

`IEntityLifecycleProcessor` は、Auditing（監査）や Soft Delete（論理削除）の機能を処理するためのインターフェースです。
これらの機能は `PersistenceOptions` の設定（`EnableAuditing`, `EnableSoftDelete`）によってオプトイン（選択式）での利用が想定されていました。
しかし、設定がOFFの場合、DIコンテナに `IEntityLifecycleProcessor` の実装が登録されず、コンストラクタで非Null制約（`IEntityLifecycleProcessor processor`）を課している `EfCoreRepository` のインスタンス化時に `InvalidOperationException` が発生してアプリケーションがクラッシュする問題が浮上しました。
この矛盾を回避するために一時的に依存関係を Nullable (`IEntityLifecycleProcessor? processor = null`) に戻した結果、アーキテクチャ監査の「Dependency Inversion と Fail-Fast」の原則に背く結果となっていました。

## 4. Decision (決定事項)

コンストラクタの Nullable 排除（アーキテクチャ要件）と、機能のオプトイン設計（ビジネス要件）を両立させるため、**Null Object Pattern** を採用します。

具体的には、何もしない（No-Operation）実装である `NoOpEntityLifecycleProcessor` を定義し、Auditing や SoftDelete が無効化されている場合のデフォルトの依存関係としてDIコンテナに登録します。

**設計詳細（DI登録ロジック）**:

```csharp
if (options.EnableAuditing || options.EnableSoftDelete)
{
    // 本物の処理を行うプロセッサ（TryAddScopedなので先に登録されたものが優先される）
    services.TryAddScoped<IEntityLifecycleProcessor, EntityLifecycleProcessor>();
}

// ... other registrations ...

// 最後にフォールバックとしてNo-Opプロセッサを登録する（誰も登録していなければこれが使われる）
services.TryAddScoped<IEntityLifecycleProcessor, NoOpEntityLifecycleProcessor>();
```

**設計詳細（リポジトリのコンストラクタ）**:

```csharp
public partial class EfCoreRepository<TEntity>(
    DbContext context,
    ILogger<EfCoreRepository<TEntity>> logger,
    ICursorSerializer cursorSerializer,
    IEntityLifecycleProcessor processor // 👈 非Null制約（?を排除）
)
```

## 5. Alternatives Considered (代替案の検討)

- **Option 1: コンストラクタ引数を Nullable にする (`IEntityLifecycleProcessor?`)**
    - **Approach**: DIで解決できない場合は `null` を受け取り、呼び出し側で `_processor?.ProcessBulkUpdate()` のように安全呼び出し演算子を使用する。
    - **Rejected Reason**: リポジトリの各所で `NullReferenceException` のリスクを意識し、冗長な Null チェック（防御的プログラミング）を強いることになります。また、アーキテクチャ監査の Fail-Fast 原則に違反します。

- **Option 2: EntityLifecycleProcessor 内で機能をON/OFF判定する**
    - **Approach**: 常に本物の `EntityLifecycleProcessor` をDIに登録し、そのクラスの内部で `PersistenceOptions` を読み込んで処理をスキップする。
    - **Rejected Reason**: `EntityLifecycleProcessor` 自体が `IAuditProvider` などの他の依存関係をコンストラクタで要求するため、Auditing がOFFで `IAuditProvider` が未登録の場合にDI構築エラーが発生します。依存関係の連鎖的な複雑化を招くため却下しました。

## 6. Consequences & Mitigation (結果と緩和策)

- **Positive (メリット)**:
    - リポジトリ群から `Nullable` 制約と冗長な Null チェックを完全に排除でき、コードが堅牢かつクリーンになりました。
    - 「何もしない」という振る舞いが `NoOpEntityLifecycleProcessor` という明確な型としてカプセル化されました。
- **Negative (デメリット)**:
    - 常にコンテナからインスタンスを解決するため、極めて微小なオブジェクト生成コスト（NoOpインスタンス）が発生します。
- **Mitigation (緩和策)**:
    - `NoOpEntityLifecycleProcessor` は状態を持たない `sealed` クラスであり、インスタンス化のコストは無視できるレベル（GCの第0世代で即座に回収される）であるため、実質的な緩和策は不要です。必要に応じて `Singleton` 登録への変更も容易です。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- `NoOpEntityLifecycleProcessor` は内部に状態を持たず、例外もスローしない安全な設計です。
- DIによるスコープの意図しない上書きを防ぐため、登録には `TryAddScoped` を活用し、登録順序（本物のプロセッサの登録を優先）を正しく担保しています。

## 8. Implementation References (参考リンク)

- `src/BuildingBlocks/Persistence/EFCore/Services/NoOpEntityLifecycleProcessor.cs`: Null Object Pattern の本体。
- `src/BuildingBlocks/Persistence/EFCore/DependencyInjection/ServiceCollectionExtensions.cs`: DIの条件分岐とフォールバック登録。
- `src/BuildingBlocks/Persistence/EFCore/Repositories/EfCoreRepository.cs`: 非Null化されたコンストラクタとプロパティ。

---

# 補足（Pending Issues & Observations）

本日の作業の中で、以下の2点の未解決課題・違和感がユーザーから指摘されました。これらは今後のタスクとして対処が必要です。

1. **`IUnitOfWork<TDbContext>` の抽象漏れ（Abstraction Leak）未解決**
    - 監査レポートにて「アプリケーション層に露出するインターフェース `IUnitOfWork` にEFCore依存のGeneric型 `<TContext>` が漏出している」との指摘がありましたが、まだ修正が完了していません。

2. **`WarnIfInsecureCursor` メソッドの実装上の違和感**
    - 本番環境での非セキュアなカーソルシリアライザーの使用を警告する処理ですが、現在のDIの `Options` パイプライン内での検証アプローチに対して、ユーザーより懸念（ちょっと問題がある感覚）が示されています。設計の再考が必要です。
