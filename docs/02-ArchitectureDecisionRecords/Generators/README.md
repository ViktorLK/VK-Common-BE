# Architecture Decision Records (ADR) - Index

このディレクトリには、VK.Blocks.Generators モジュールの主要な設計決定を記録した ADR が含まれています。

## 📚 ADR 一覧

### Core Architecture (コアアーキテクチャ)

---

### Language Support (言語機能サポート)

#### [ADR-001: Support Modern C# Features in .NET Standard 2.0 Generators via Polyfills](/docs/02-ArchitectureDecisionRecords/Generators/adr-001-support-modern-csharp-features-in-netstandard20-generators-via-polyfills.md)

**Status**: ✅ Accepted  
**概要**: `.netstandard 2.0` 環境で `record` や `init` 等のモダンC#機能を有効化するため、外部依存を持たないコンパイル時 Polyfill（`IsExternalInit`）を採用  
**キーワード**: Polyfills, netstandard2.0, Zero Dependency, Modern C#

---

#### [ADR-002: Establish Incremental Source Generators Pattern for Infrastructure Boilerplate](/docs/02-ArchitectureDecisionRecords/Generators/adr-002-establish-incremental-source-generators-pattern-for-infrastructure-boilerplate.md)

**Status**: ✅ Accepted  
**概要**: インフラストラクチャーのボイラープレート（DI登録や定数生成）に対して、完全な増分処理（Strictly Incremental）によるコンパイル時コード生成アーキテクチャを確立  
**性能向上**: Zero Runtime Overhead, リフレクション排除  
**キーワード**: Incremental Source Generators, Compile-Time Configuration, Zero Overhead

---

### Observability (可観測性)

#### [ADR-003: Automate Observability Infrastructure via Source Generation](/docs/02-ArchitectureDecisionRecords/Generators/adr-003-automate-observability-infrastructure-via-source-generation.md)

**Status**: ✅ Accepted  
**概要**: `[VKBlockDiagnostics]` 属性の付与だけで、分散トレーシング（`ActivitySource`）とメトリクス（`Meter`）の配備を自動生成し、命名の一貫性とパフォーマンスを担保  
**キーワード**: Observability, Telemetry, ActivitySource, Meter, Automation

---

## 🎯 ADR の読み方ガイド

### Source Generator 設計の理解用

1. **ADR-002**: `CompilationProvider.Combine` の禁止や `WhereNotNull` の活用など、IDE とビルドパイプラインを重くしない「真の Incremental Pipeline」の概念
2. **ADR-001**: ターゲットフレームワークの制約をバイパスしつつ、外部NuGetパッケージに依存しないゼロデペンデンシー設計の維持手法

### インフラストラクチャ自動化の理解用

1. **ADR-003**: 運用監視（Observability）の導入障壁を下げるためのメタデータ駆動アプローチと、生成パターンの実例

---

## 🔗 関連ドキュメント

- 今後のアーキテクチャ監査や横断的なガイドが生成された場合、ここに追加されます。

---

**Last Updated**: 2026-03-06  
**Total ADRs**: 3
