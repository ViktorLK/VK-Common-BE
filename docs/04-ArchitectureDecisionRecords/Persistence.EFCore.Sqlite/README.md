# Architecture Decision Records (ADR) - Index

このディレクトリには、VK.Blocks.Persistence.EFCore.Sqlite モジュールの主要な設計決定を記録した ADR が含まれています。

## 📚 ADR 一覧

### Core Architecture

#### [ADR-001: Independent Sqlite Persistence Block](./adr-001-independent-sqlite-persistence-block.md)

**Status**: ✅ Accepted  
**概要**: SQLite データベースの構成や NuGet 依存関係を他のデータベースプロバイダから分離し、依存関係を極小化した独立モジュールとしてパッケージングする設計。  
**キーワード**: SQLite Provider, DbContext Configurator, Dependency Isolation

---

### Type Mapping & Engine Stability

#### [ADR-002: Standardized DateTimeOffset Storage via Model Convention (Abandoning EF1001 Internal APIs)](./adr-002-standardized-datetimeoffset-storage-via-model-convention.md)

**Status**: ✅ Accepted  
**概要**: EF Core の非公開内部 API（`EF1001`）であった式ツリー変換器を廃止し、`IVKModelConventionContributor` と `DateTimeOffsetToStringConverter` による標準的な ISO 8601 文字列変換規約へ移行。  
**キーワード**: Model Convention, DateTimeOffset, EF1001 Elimination, ISO 8601, Cursor Pagination

---

## 🎯 ADR の読み方ガイド

1. **モジュール分離の理解**: **ADR-001** で、なぜ SQLite 専用の独立 BuildingBlock を設けたかの背景を理解してください。
2. **型マッピングと安定性の理解**: **ADR-002** で、非標準型である `DateTimeOffset` を SQLite で安全かつ永続的にソート・比較するための規約設計を把握してください。

## 🔗 関連ドキュメント
- [Persistence Module Manifest](/src/BuildingBlocks/Persistence/.prompts/module-manifest.md)
- [Persistence.EFCore ADRs](/docs/04-ArchitectureDecisionRecords/Persistence.EFCore/README.md)

**Last Updated**: 2026-09-04  
**Total ADRs**: 2
