# アーキテクチャ監査レポート: EFCore Persistence (2026-02-18)

## 📊 監査サマリー (Audit Summary)

- **総合スコア**: 88/100点
- **対象レイヤー判定**: Infrastructure Layer (EF Core Persistence Building Block)
- **総評 (Executive Summary)**: Clean Architectureに高度に準拠した、教科書的な実装です。CQSの分離、Interceptorによる横断的関心事の処理、セキュリティへの配慮など、非常に高品質です。唯一の懸念は、`IUnitOfWork` における抽象化の漏洩と、一部のDI設計における曖昧さです。

## 🚨 重大なアーキテクチャの懸念事項 (Critical Architectural Smells)

- ❌ **[DIP Violation]**: `UnitOfWork.cs`, `ServiceCollectionExtensions.cs` - `IUnitOfWork<TDbContext>` という定義において、型パラメータ `TDbContext` が EF Core 固有の `DbContext` を制約として持っています。これにより、Application Layer が間接的に EF Core に依存することになり、技術詳細の隠蔽が不完全です。
- ❌ **[Dependency Clarity]**: `EfCoreRepository.cs` - コンストラクタ引数（`ICursorSerializer` 等）が nullable (`?`) として定義されていますが、実際には必須の依存関係です。DIコンテナの構成ミスがコンパイル時に検出できず、実行時エラーや予期せぬ挙動（デフォルト実装へのフォールバックなど）につながるリスクがあります。

## 🛡️ 非機能要件とセキュリティ (Non-Functional Requirements & Security)

- 🔒 **[セキュリティ]**: `SecureCursorSerializer` は HMAC-SHA256 署名と定数時間比較を実装しており、タイミング攻撃などのセキュリティリスクに対して堅牢です。一方で、`SimpleCursorSerializer` が本番環境で誤用されるリスクがあり、自動検出・警告の仕組みが必要です。
- 🔒 **[パフォーマンス]**: Offset Pagination (`GetPagedAsync`) において、常に `COUNT(*)` が実行されるため、大規模テーブルではパフォーマンスのボトルネックになる可能性があります。

## 🧪 テスト容易性と疎結合性 (Testability & Decoupling)

- ⚙️ **[テスト容易性]**: `EfCoreReadRepository` (Query) と `EfCoreRepository` (Command) が明確に分離されており（CQS）、テスト戦略を立てやすい構造になっています。
- ⚙️ **[疎結合性]**: `IUnitOfWork` の型パラメータ問題を除けば、各コンポーネントはインターフェースを通じて疎結合に保たれています。

## 🔭 可観測性の準拠度 (Observability Readiness)

- 📡 **[運用監視]**: `IDbCommandInterceptor` が未実装であり、スロークエリの検出やSQLの構造化ログ出力といったデータベースレベルの可観測性が不足しています。

## ⚠️ コード品質とコーディング規約のリスク (Code Quality & Standard Risks)

- ⚠️ **[SRP/Registration]**: `ServiceCollectionExtensions.cs` にて、`Configure()` が重複して呼び出されています。設定が二重適用される無駄があります。
- ⚠️ **[DRY/Validation]**: `GetCursorPagedAsync` におけるページサイズの検証ロジックがインラインで記述されており、`PaginationValidator` に集約されていません。また、マジックナンバーや文字列リテラルが使用されています。
- ⚠️ **[Documentation]**: `UnitOfWork.cs` のXMLコメントがコピペミスにより誤ったクラスを参照しています。
- ⚠️ **[Encapsulation]**: `EfCoreExpressionCache` のフィールドが `public` で公開されており、カプセル化違反の状態です。

## ✅ 評価ポイント (Highlights / Good Practices)

- **CQS (Command Query Separation)**: Read/Write リポジトリの明確な分離。
- **Interceptor Pattern**: `AuditingInterceptor` と `SoftDeleteInterceptor` による、ビジネスロジックに介入しないクリーンな実装。
- **Bulk Operation Integrity**: バルク操作（`ExecuteUpdate`）時にも監査ロジックを適用する仕組み（`IEntityLifecycleProcessor`）が担保されています。
- **Type-Safe Reflection Cache**: `EfCoreMethodInfoCache` によるコンパイル時安全なリフレクション代替手法。
- **Secure Cursor**: 本格的な暗号化署名付きカーソルの実装。

## 💡 改善ロードマップ (Evolutionary Roadmap)

1. **最優先対応 (Immediate Action)**:
    - `IUnitOfWork<TDbContext>` から型パラメータを除去し、`IUnitOfWork` として定義し直す（Application Layerの純粋化）。
    - `EfCoreRepository` のコンストラクタ引数の nullable を廃止し、必須依存として定義する。
    - `ServiceCollectionExtensions` の重複 `Configure` 呼び出しを削除する。

2. **リファクタリング提案 (Refactoring)**:
    - ページネーションのバリデーションロジックを `PaginationValidator` に統一する。
    - 本番環境で `SimpleCursorSerializer` が登録されている場合に警告ログを出力する仕組みを追加する。
    - `EfCoreExpressionCache` のフィールドを `private` に修正する。

3. **推奨される学習トピック (Learning Suggestions)**:
    - **Options Pattern Variations**: `IOptions`, `IOptionsSnapshot`, `IOptionsMonitor` の違いと適切な使い分けについて。
    - **Specification Pattern**: `Expression<Func<T, bool>>` を直接渡すのではなく、クエリ仕様をオブジェクトとしてカプセル化する手法。
    - **Advanced EF Core Interceptors**: `IDbCommandInterceptor` を用いた高度なデータベース可観測性の実装。
