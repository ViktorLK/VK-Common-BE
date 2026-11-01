# アーキテクチャ監査レポート: EFCore Persistence (2026-02-12)

## 📊 監査サマリー (Audit Summary)

- **総合スコア**: 72/100点
- **対象レイヤー判定**: Infrastructure Layer (Persistence Implementation)
- **総評 (Executive Summary)**: RepositoryパターンやUnit of Work、Interceptorといったエンタープライズパターンの適用は堅実であり、評価できます。しかし、DIコンテナの構成における致命的なミス（実行時クラッシュのリスク）や、抽象化層におけるEF Core固有型の漏洩など、保守性と信頼性を損なう構造的な欠陥が見受けられます。

## 🚨 重大なアーキテクチャの懸念事項 (Critical Architectural Smells)

- ❌ **[Dependency Injection]**: `ServiceCollectionExtensions.cs` - `UnitOfWork` のコンストラクタが `DbContext` を要求するのに対し、DI登録では具体型（`TContext`）のみが登録されており、基底クラスの登録が欠落しています。これにより、実行時に `InvalidOperationException` が発生し、アプリケーションがクラッシュします（Blocking Issue）。

## 🛡️ 非機能要件とセキュリティ (Non-Functional Requirements & Security)

- 🔒 **[パフォーマンス/ロジック]**: `EfCoreRepository.cs` の `ExecuteUpdateAsync` における Expression Tree の構築ロジックに誤りがあります。`Expression.Parameter` を再利用せずに都度生成しているため、論理的な等価性が崩れ、クエリ変換の失敗や予期しないSQL生成を引き起こす可能性があります。
- 🔒 **[国際化 (I18N)]**: エラーメッセージ（例：「既にアクティブなトランザクションが存在します」）がハードコーディングされており、多言語対応の妨げとなります。

## 🧪 テスト容易性と疎結合性 (Testability & Decoupling)

- ⚙️ **[疎結合性の欠如]**: `IBaseRepository` インターフェースにおいて、`Func<IQueryable<T>, IQueryable<T>>` がパラメータとして公開されています。これはEF Core（LINQ to Entities）への依存を抽象化層に漏洩させており、将来的なデータアクセス技術の変更（DapperやMongoDBへの移行など）を困難にします。

## 🔭 可観測性の準拠度 (Observability Readiness)

- 📡 **[運用監視]**: `ExecuteUpdateAsync` などの一括操作において、構造化ログ（`ILogger`）による出力が実装されていません。影響を受けた行数や操作の詳細が記録されないため、本番環境でのトラブルシューティングが困難です。
- 📡 **[例外処理]**: `DbUpdateException` 等のデータベース固有例外に対するハンドリングが欠如しており、Fail-Fastの原則に反しています。

## ⚠️ コード品質とコーディング規約のリスク (Code Quality & Standard Risks)

- ⚠️ **[YAGNI (不要機能)]**: `PersistenceOptions` に定義された `CreatedAtPropertyName` は、現在どのコードからも参照されておらず、デッドコードとなっています。
- ⚠️ **[リソース管理]**: `DbContext` (Scoped) のファクトリデリゲート内で Scoped ライフサイクルの `AuditingInterceptor` を解決しています。通常は動作しますが、`DbContext` の生成タイミングによっては予期せぬライフサイクルミスマッチを引き起こす可能性があります。

## ✅ 評価ポイント (Highlights / Good Practices)

- **Repository & Unit of Work**: インターフェース分離の原則 (ISP) と依存性逆転の原則 (DIP) に準拠した、教科書的な実装です。
- **Interceptors (AOP)**: 監査 (Auditing) と論理削除 (Soft Delete) のロジックをインターセプターとして分離しており、単一責任の原則 (SRP) を満たしています。
- **Pagination Strategy**: Offset と Cursor の両方のページネーションをサポートしており、大規模データセットへの対応も考慮されています。
- **Dynamic Expression Trees**: `ExecuteUpdateAsync` において、コンパイル時に動的に式木を構築する高度なメタプログラミング技術が使用されています（実装バグはあるものの、アプローチ自体は評価できます）。

## 💡 改善ロードマップ (Evolutionary Roadmap)

1. **最優先対応 (Immediate Action)**:
    - `ServiceCollectionExtensions` における `UnitOfWork` のDI登録ミスを修正し、`services.AddScoped<DbContext>(sp => sp.GetRequiredService<TContext>())` を追加する。
    - `Expression.Parameter` の再利用バグを修正し、正しい式木が構築されるようにする。

2. **リファクタリング提案 (Refactoring)**:
    - `IBaseRepository` から `IQueryable` への依存を排除し、Specification パターン (`ISpecification<T>`) を導入してクエリロジックをカプセル化する。
    - `UnitOfWork` の例外メッセージをリソースファイルに移し、ハードコーディングを解消する。
    - 一括操作メソッドに `ILogger` を導入し、実行結果を構造化ログとして出力する。

3. **推奨される学習トピック (Learning Suggestions)**:
    - **Specification Pattern**: EF Core への依存を隠蔽し、テスト可能なクエリ仕様を定義する方法について。
    - **Expression Trees**: 正しいパラメータの再利用と、動的なLINQクエリの構築方法の深掘り。
