# 詳細アーキテクチャ監査報告書：VK.Blocks.Persistence.EFCore

## 📊 総合スコア (Total Score: 85)

本モジュールは、成熟した .NET アーキテクチャスタイルを示しており、関心事の分離 (SoC) と依存性逆転の原則 (DIP) を厳格に遵守しています。現代的な C# 機能を活用し、抽象化層を通じてビジネスロジックを具体的なデータアクセス技術から効果的に切り離しています。主な改善点は、一部の高度な機能（例：カーソルページネーション）の実装の複雑さと、特定のエンタープライズパターン（例：分散トランザクション）への明示的なサポートが現状では基礎的である点にあります。全体として、これは高品質で保守性が高く、拡張が容易な永続化層の実装です。

---

## ✅ アーキテクチャの強み (Architectural Strengths)

- **DRY (Don't Repeat Yourself) & カプセル化**:
    - `EfCoreRepository<TEntity>` は汎用的な CRUD 操作をカプセル化し、重複コードを大幅に削減しています。これにより、開発効率とコードの一貫性が向上します。
    - `BaseDbContext` はグローバルフィルター（論理削除）と並行性トークンの構成を一元管理し、すべてのエンティティ設定で同じロジックを繰り返すことを回避しています。
    - **ビジネスメリット**: コードの再利用率を高め、保守コストを削減し、グローバルポリシーの一貫性を保証します。

- **依存性逆転の原則 (DIP) & モジュール化**:
    - モジュールは具体的な実装ではなく、抽象インターフェース（`IUnsafeContext`, `IEntityLifecycleProcessor`, `IBaseRepository` など）に大きく依存しています。
    - `ServiceCollectionExtensions` を通じて、明確な依存性注入 (DI) の登録ポイントを提供しています。
    - **ビジネスメリット**: モジュールのテスト容易性と交換可能性を強化し、ビジネス層が EF Core の詳細に直接依存することを防ぎます。

- **単一責任の原則 (SRP)**:
    - `EntityLifecycleProcessor` はエンティティのライフサイクルイベント（監査、論理削除）を専門に担当し、`DbContext` の SaveChanges ロジックから切り離しています。
    - `EfCoreExpressionCache` は式木キャッシュを専門に担当しています。
    - **ビジネスメリット**: 各クラスの役割が明確であり、機能変更時の副作用リスクを最小限に抑えます。

- **パフォーマンス最適化 (Performance Optimization)**:
    - **式木キャッシュ**: `EfCoreExpressionCache` はコンパイル済みのラムダ式をキャッシュし、高頻度の呼び出しシナリオでパフォーマンスを大幅に向上させます。
    - **一括操作のサポート**: [`ExecuteUpdateAsync`](/src/BuildingBlocks/Persistence/EFCore/EfCoreRepository.cs#L244-L284) および [`ExecuteDeleteAsync`](/src/BuildingBlocks/Persistence/EFCore/EfCoreRepository.cs#L286-L303) のカプセル化により、大量のエンティティをメモリにロードすることなく、データベースレベルで直接操作を実行できます。
    - **NoTracking サポート**: 読み取り専用シナリオ向けに `AsNoTracking` オプションを提供し、ChangeTracker のオーバーヘッドを削減します。

- **現代的なアーキテクチャパターンのサポート**:
    - **インターセプターパターン (Interceptor Pattern)**: [`AuditingInterceptor`](/src/BuildingBlocks/Persistence/EFCore/Interceptors/AuditingInterceptor.cs) と [`SoftDeleteInterceptor`](/src/BuildingBlocks/Persistence/EFCore/Interceptors/SoftDeleteInterceptor.cs) を使用して `SaveChanges` パイプラインに横断的関心事を注入します。これは監査と論理削除を処理するエレガントな方法です。
    - **リポジトリパターン (Repository Pattern)**: 標準的なジェネリックリポジトリ実装を提供し、基盤となる ORM の複雑さを隠蔽します。

---

## ⚠️ アーキテクチャ上のリスク (Architectural Risks & Smells)

- **カーソルページネーションの複雑さ (Complexity of Cursor Pagination)**:
    - **リスク**: [`GetCursorPagedAsync`](/src/BuildingBlocks/Persistence/EFCore/Repositories/EfCoreReadRepository.Query.cs#L58-L121) および [`QueryableExtensions.OrderByCursorDirection`](/src/BuildingBlocks/Persistence/EFCore/Extensions/QueryableExtensions.cs) のロジックは比較的複雑であり、特に逆方向ページング時のソート反転やメモリ内反転が含まれます。
    - **結果**: 複雑なロジックは理解と保守の難易度を高め、特定のソートの組み合わせでバグを引き起こす可能性があります。また、クライアントが Base64 エンコードされたカーソル文字列を転送することに依存しているため、データ構造が変更されるとカーソルが無効になる可能性があります。

- **論理削除のグローバルフィルターの落とし穴**:
    - **リスク**: `ApplyGlobalFilters` は論理削除フィルターを自動的に適用しますが、直接 SQL 操作を行う `ExecuteDeleteAsync` や特定のクエリで誤って `IgnoreQueryFilters` を使用すると、削除済みデータを意図せず操作する可能性があります。
    - **結果**: データの一貫性リスクがあり、ビジネスロジックが「削除済み」であるはずのデータを誤って処理する可能性があります。

- **IUnsafeContext インターフェース設計**:
    - **リスク**: `IUnsafeContext` はマーカーインターフェースとして存在し、現在 `TODO` を含んでおり、意図が不明確です。これは YAGNI (You Aren't Gonna Need It) 違反、または未完成の設計である可能性があります。
    - **結果**: 誤用される可能性や、将来的な安全でない操作の「バックドア」として機能し、カプセル化を破壊するリスクがあります。

- **トランザクション管理の依存**:
    - **リスク**: `UnitOfWork` は EF Core の `IDbContextTransaction` に強く依存しています。アダプター経由でカプセル化されていますが、複数の DbContext や異種データソースにまたがる分散トランザクションが必要な場合、現在の実装では不十分な可能性があります。
    - **結果**: マイクロサービスや複雑な分散シナリオでは、より複雑なトランザクション調整メカニズム（CAP 定理や Saga パターンなど）の導入が必要になる可能性があります。現在の実装は主に単体データベーストランザクションを対象としています。

---

## 💡 演進ロードマップ (Evolutionary Roadmap)

- **ページネーション戦略の最適化**:
    - Base64 文字列だけに依存するのではなく、メタデータ（ソートキー値、タイムスタンプなど）を含む専用の `Cursor` オブジェクトまたは構造体の導入を推奨します。これにより、カーソルの堅牢性が向上します。
    - 異なるページネーションアルゴリズムをサポートするために、ページネーションロジックを独立した戦略クラス (`PaginationStrategy`) に抽出することを検討してください。

- **`IUnsafeContext` の完成または削除**:
    - `IUnsafeContext` が `ExecuteSqlRaw` などの危険な操作を提供することを意図している場合は、そのメソッドを明確に定義し、適切なセキュリティ警告（Roslyn アナライザー警告など）を追加してください。明確な要件がない場合は、YAGNI 原則に従って削除することを推奨します。

- **一括操作の安全性強化**:
    - `ExecuteDeleteAsync` において、`forceDelete` フラグのチェックに加えて、より厳密な型制約やビルダーパターンを導入し、「削除済みデータを含む」または「未削除データのみ」を明示的に指定することを強制することを検討してください。これにより、デフォルト動作による曖昧さを回避できます。

- **仕様パターン (Specification Pattern) の導入**:
    - 現在のリポジトリ層はクエリに `Expression<Func<TEntity, bool>>` を依存しています。ビジネスロジックが複雑化するにつれて、クエリロジックを再利用可能なオブジェクトとしてカプセル化する Specification Pattern を導入し、ビジネス層と LINQ 式の結合をさらに緩めることを推奨します。
