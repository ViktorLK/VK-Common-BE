# アーキテクチャ監査レポート: EFCore Persistence (2026-02-14)

## 📊 監査サマリー (Audit Summary)

- **総合スコア**: 85/100点
- **対象レイヤー判定**: Infrastructure Layer (Persistence Implementation)
- **総評 (Executive Summary)**: 成熟した.NETアーキテクチャスタイルを示しており、関心事の分離と依存性逆転の原則を厳格に遵守しています。特に、汎用的なCRUD操作のカプセル化とパフォーマンスへの配慮は優れていますが、カーソルページネーションなどの高度な機能の実装における複雑さが、将来的な保守リスクとなる可能性があります。

## 🚨 重大なアーキテクチャの懸念事項 (Critical Architectural Smells)

- ❌ **[Encapsulation/Safety]**: `IUnsafeContext` - マーカーインターフェースとして存在し、その意図が不明確です。安全でない操作（生SQLなど）へのバックドアとなるリスクがあり、カプセル化を破壊する可能性があります。
- ❌ **[Data Consistency]**: `ApplyGlobalFilters` - 論理削除フィルターが自動適用されますが、`ExecuteDeleteAsync` や `IgnoreQueryFilters` を併用する際に、誤って論理削除済みのデータを操作してしまうリスクがあります（論理的な一貫性の欠如）。

## 🛡️ 非機能要件とセキュリティ (Non-Functional Requirements & Security)

- 🔒 **[パフォーマンス]**: `EfCoreExpressionCache` による式木キャッシュや、`AsNoTracking` のサポートなど、パフォーマンス最適化への意識が高い実装です。
- 🔒 **[信頼性]**: カーソルページネーションの実装（特に逆方向ページング）が複雑であり、Base64文字列への依存はデータ構造の変更に弱いため、堅牢性に懸念があります。

## 🧪 テスト容易性と疎結合性 (Testability & Decoupling)

- ⚙️ **[疎結合性]**: 抽象インターフェース（`IEntityLifecycleProcessor`, `IBaseRepository`）への依存が徹底されており、ビジネスロジックは具体的なデータアクセス技術から保護されています。

## 🔭 可観測性の準拠度 (Observability Readiness)

- 📡 **[情報なし]**: 特記事項なし。

## ⚠️ コード品質とコーディング規約のリスク (Code Quality & Standard Risks)

- ⚠️ **[Complexity]**: カーソルページネーションのロジックが複雑で、保守が困難になりつつあります。
- ⚠️ **[Maintainability]**: `UnitOfWork` が EF Core の `IDbContextTransaction` に依存しており、分散トランザクションが必要になった場合の拡張性に制限があります。

## ✅ 評価ポイント (Highlights / Good Practices)

- **DRY & Encapsulation**: 汎用的なCRUD操作とグローバルフィルターの一元管理により、コードの重複を排除しています。
- **SRP (Single Responsibility)**: `EntityLifecycleProcessor` や `EfCoreExpressionCache` など、責務が明確に分離されたクラス設計がなされています。
- **Modern Patterns**: InterceptorによるAOPの実装や、Specificationパターン（推奨段階）への準備が見られます。

## 💡 改善ロードマップ (Evolutionary Roadmap)

1. **最優先対応 (Immediate Action)**:
    - `IUnsafeContext` の意図を明確にするか、YAGNI原則に従って削除する。危険な操作を行う場合は、適切な警告（Roslyn Analyzer等）を伴う設計にする。
    - `ExecuteDeleteAsync` の利用において、対象データを明示的に強制する（削除済みを含むかどうか）仕組みを導入し、事故を防ぐ。

2. **リファクタリング提案 (Refactoring)**:
    - カーソルページネーションにおいて、Base64文字列だけでなく、構造化された `Cursor` オブジェクトを導入し、堅牢性を高める。
    - ページネーションロジックを `PaginationStrategy` として分離することを検討する。

3. **推奨される学習トピック (Learning Suggestions)**:
    - **Specification Pattern**: LINQ式への直接依存を減らし、クエリロジックを再利用可能な仕様として定義する方法。
    - **Distributed Transactions**: マイクロサービス環境におけるSagaパターンなど、単一DBトランザクションを超えた整合性管理について。
