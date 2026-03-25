# Architecture Decision Records (ADR) - Index

このディレクトリには、VK.Blocks.Blob モジュールの主要な設計決定を記録した ADR が含まれています。

## 📚 ADR 一覧

#### [ADR-001: Standardizing on Azure Blob Storage API for Portability and Unified Development](./adr-001-optimizing-blob-storage-architecture-for-portability-and-security.md)

**Status**: ✅ Accepted  
**概要**: Local FileSystem 実装を削除し、開発環境を含むすべての環境で Azure Blob API (Azurite) に一本化する決定。Singleton クライアント、Rule 14 遵守を含む。  
**キーワード**: Unified Storage API, Azurite, Singleton Lifecycle, Enum Segregation

---

#### [ADR-002: Decoupling Blob Abstractions from Azure Storage Implementation](./adr-002-decoupling-blob-abstractions-from-azure-storage-implementation.md)

**Status**: ✅ Accepted  
**概要**: インターフェース（抽象）と Azure SDK（実装）を別プロジェクトに分離し、依存関係の伝播を防止する決定。  
**キーワード**: Dependency Inversion, Clean Abstractions, Zero Transient Dependencies

#### [ADR-003: Adoption of Pure Infrastructure Model and Decoupling from Core Context](./adr-003-pure-infrastructure--core-context-.md)

**Status**: ✅ Accepted  
**概要**: IUserContext や IDateTime への依存を排除し、ライブラリをビジネスコンテキストから分離。汎用性と再利用性を向上。  
**キーワード**: Pure Infrastructure, Zero Dependency, Single Responsibility

---

#### [ADR-004: Integration of Standard Observability Patterns using VKBlockDiagnostics](./adr-004-standardized-observability-integration.md)

**Status**: ✅ Accepted  
**概要**: [VKBlockDiagnostics] ソースジェネレーターを活用し、分散トレーシング（Tracing）とメトリクス（Metrics）を標準計装。  
**キーワード**: OpenTelemetry, Distributed Tracing, Source Generators

---

## 🎯 ADR の読み方ガイド

### アーキテクチャと純粋設計の理解用

1. **ADR-001**: なぜカスタム実装を捨て、成熟したエミュレータ（Azurite）に一本化すべきなのか、その判断基準を学ぶことができます。
2. **ADR-002**: インフラ依存をいかにして隠蔽し、再利用性の高い抽象レイヤーを構築するかについての実践的な手法を理解できます。
3. **ADR-003**: インフラライブラリがいかにしてアプリケーションコンテキストから独立を保つべきか、その設計思想を理解できます。

### 運用と可観測性の理解用

1. **ADR-004**: OpenTelemetry を用いた標準的な計装方法と、ソースジェネレーターによるボイラープレートの削減手法について学べます。

**Last Updated**: 2026-03-24  
**Total ADRs**: 4
