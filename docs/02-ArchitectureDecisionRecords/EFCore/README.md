# Architecture Decision Records (ADR) - Index

このディレクトリには、VK.Blocks.Persistence.EFCore モジュールの主要な設計決定を記録した ADR が含まれています。

## 📚 ADR 一覧

### Core Architecture (コアアーキテクチャ)

#### [ADR-001: Hybrid Auditing Strategy](./adr-001-hybrid-auditing.md)

**Status**: ✅ Accepted  
**概要**: 標準 CRUD 操作では Interceptor、Bulk 操作では Repository で明示的に監査処理を実行するハイブリッド戦略  
**性能向上**: 90倍（Bulk 操作）  
**キーワード**: Interceptor, Bulk Operations, IEntityLifecycleProcessor

---

### Performance Optimization (性能最適化)

#### [ADR-002: Static Generic Caching for Zero-Overhead Metadata](./adr-002-static-generic-caching.md)

**Status**: ✅ Accepted  
**概要**: C# の静的ジェネリッククラスを活用し、型メタデータ（`IAuditable`, `ISoftDelete` の実装有無）をゼロコストでキャッシュ  
**性能向上**: 62倍（50ns → <1ns）  
**キーワード**: Static Generic, CLR, Reflection Optimization

#### [ADR-004: Expression Compilation Caching for High-Performance Cursor Pagination](./adr-004-expression-caching.md)

**Status**: ✅ Accepted  
**概要**: `ExpressionEqualityComparer` を使用し、Expression Tree のコンパイル結果をキャッシュ  
**性能向上**: 91倍（10,000ms → 109ms）  
**キーワード**: Expression Tree, ExpressionEqualityComparer, Memoization

#### [ADR-008: MethodInfo Caching for Bulk Operations](./adr-008-methodinfo-caching.md)

**Status**: ✅ Accepted  
**概要**: 静的ジェネリッククラスで `SetProperty` の MethodInfo をキャッシュ  
**性能向上**: 100倍（50μs → <1μs）  
**キーワード**: MethodInfo, Reflection, Micro-Optimization

#### [ADR-012: EF Core Bulk Optimization and Adapter Pattern for .NET 10](./adr-012-efcore-bulk-update-refactoring.md)

**Status**: ✅ Accepted  
**概要**: Bulk操作の共通監査処理とEF Coreの動的プロパティ更新に対し、Adapter PatternとSource Generatorsを採用してゼロアロケーション化とOCPを維持  
**性能向上**: 高速ロギングとリフレクション排除によるアロケーション削減  
**キーワード**: Adapter Pattern, Source Generators, Bulk Operations, OCP

---

### Scalability (スケーラビリティ)

#### [ADR-003: Cursor-Based Pagination with Bidirectional Scrolling](./adr-003-cursor-pagination.md)

**Status**: ✅ Accepted  
**概要**: Offset Pagination の Deep Pagination 問題を解決する Cursor Pagination の実装  
**性能向上**: 1,250倍（10,000ページ目で 5,000ms → 4ms）  
**キーワード**: Cursor Pagination, Expression Tree, Bidirectional Scrolling

#### [ADR-005: IAsyncEnumerable for Memory-Efficient Data Streaming](./adr-005-async-enumerable-streaming.md)

**Status**: ✅ Accepted  
**概要**: C# 8.0 の `IAsyncEnumerable` を使用し、大規模データセットをストリーミング処理  
**メモリ削減**: 95%（1GB → 50MB）  
**キーワード**: IAsyncEnumerable, yield return, Backpressure

---

### Design Patterns (設計パターン)

#### [ADR-006: Command-Query Separation (CQS) in Repository Pattern](./adr-006-cqs-repository.md)

**Status**: ✅ Accepted  
**概要**: 読み取り専用 Repository と書き込み専用 Repository を分離し、CQS 原則を適用  
**性能向上**: 33%（AsNoTracking による最適化）  
**キーワード**: CQS, CQRS, Repository Pattern

#### [ADR-007: Dynamic Global Query Filters via Reflection](./adr-007-dynamic-query-filters.md)

**Status**: ✅ Accepted  
**概要**: Reflection を使用し、`ISoftDelete` エンティティに Global Query Filter を自動適用  
**コード削減**: 90%（100行 → 10行）  
**キーワード**: Reflection, Global Query Filter, Convention over Configuration

#### [ADR-009: Cursor Serializer Abstraction (`ICursorSerializer`)](./adr-009-cursor-serializer-abstraction.md)

**Status**: ✅ Accepted  
**概要**: カーソルのシリアライズ戦略を `ICursorSerializer` インターフェースで抽象化し、Strategy パターンで差し替え可能にする。開発用 `SimpleCursorSerializer` と本番用 `SecureCursorSerializer`（HMAC-SHA256）を DI で切り替え  
**Supersedes**: ADR-003 Future Considerations §1  
**キーワード**: Strategy Pattern, ICursorSerializer, HMAC-SHA256, DIP, OCP

#### [ADR-010: Decoupling Multi-Tenancy from Auditing Infrastructure](./adr-010-decoupling-multitenancy.md)

**Status**: ✅ Accepted  
**概要**: `IAuditProvider` に混在していた `TenantId` の責務を専用の `ITenantProvider` に分離し、Multi-Tenancy と Auditing のアーキテクチャ上の疎結合性を向上させる  
**キーワード**: Separation of Concerns, Interface Segregation, Multi-Tenancy

#### [ADR-011: Null Object Pattern for IEntityLifecycleProcessor](./adr-011-entity-lifecycle-processor-null-object.md)

**Status**: ✅ Accepted  
**概要**: Auditing や SoftDelete 機能がOFFの場合のDIコンポーネント依存解決エラーを防ぐため、Null Object Pattern (`NoOpEntityLifecycleProcessor`) を採用し、アーキテクチャの Fail-Fast 原則（コンストラクタの Nullable 排除）を維持する  
**キーワード**: Null Object Pattern, Dependency Injection, Fail-Fast

---

## 🎯 ADR の読み方ガイド

### 高度な技術要素の理解用

1. **ADR-002**: C# の高度な機能（静的ジェネリック、CLRの仕組み）の活用例
2. **ADR-003**: アルゴリズム設計と Expression Tree の実践的な応用
3. **ADR-004**: Expression Tree の深い制御（`ExpressionEqualityComparer`の実装）
4. **ADR-005**: 最新の C# 非同期ストリーム処理（`IAsyncEnumerable`）の活用

### アーキテクチャ構成の理解用

1. **ADR-001**: Audit機能のハイブリッド戦略と設計思想
2. **ADR-006**: Repository Patternへの CQS（Command-Query Separation）原則の適用
3. **ADR-007**: Convention over Configuration（設定より規約）の自動適用
4. **ADR-010**: マルチテナント機能と監査インフラストラクチャの責務分離（SRP）
5. **ADR-011**: 非活性（Opt-In OFF）時のNull Object PatternによるDIクラッシュの回避

### 性能最適化のアプローチ理解用

1. **ADR-002**: マイクロ最適化（リフレクションコストの完全排除）
2. **ADR-003**: マクロ最適化（DBクエリのページネーション戦略の根本的改善）
3. **ADR-005**: メモリ使用量最適化（リスト全体バッファリングからストリーミングへの移行）
4. **ADR-012**: ゼロアロケーションでのロギングと、EF CoreネイティブビルダーのAdapter Patternによる隠蔽

---

## 📊 システム性能最適化のサマリー

| ADR     | 最適化対象           | 性能向上           | 影響範囲           |
| ------- | -------------------- | ------------------ | ------------------ |
| ADR-001 | Bulk Operations      | 90x                | 大規模データ更新   |
| ADR-002 | Type Metadata        | 62x                | 全操作             |
| ADR-003 | Pagination           | 1,250x             | 深いページ         |
| ADR-004 | Expression Compile   | 91x                | Cursor Pagination  |
| ADR-005 | Memory Usage         | 95% reduction      | 大規模データ処理   |
| ADR-006 | Read Operations      | 33%                | 読み取り専用操作   |
| ADR-007 | Query Filtering      | コード量 90%削減   | 規約ベースの適用   |
| ADR-008 | MethodInfo Lookup    | 100x               | Bulk Operations    |
| ADR-009 | Cursor Serialization | セキュリティ強化   | Cursor Pagination  |
| ADR-010 | Tenancy & Auditing   | 関心事の分離       | システム疎結合化   |
| ADR-011 | Dependency Injection | Fail-Fast の維持   | コンストラクタ     |
| ADR-012 | Bulk Operations      | アロケーション削減 | Bulk 更新/削除操作 |

---

## 🔗 関連ドキュメント

- [Architecture Audit Report](/docs/04-AuditReports/EFCore/EFCore_Persistence_20260218.md) - 包括的なアーキテクチャ評価
- [System Overview](/docs/01-Architecture/EFCore/system-overview.md) - システム全体の概要
- [Data Flow](/docs/01-Architecture/EFCore/data-flow.md) - データフロー図

---

**Last Updated**: 2026-02-25  
**Total ADRs**: 12
