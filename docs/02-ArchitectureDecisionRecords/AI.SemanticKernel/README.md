# Architecture Decision Records (ADR) - Index

このディレクトリには、VK.Blocks.AI.SemanticKernel モジュールの主要な設計決定を記録した ADR が含まれています。

## 📚 ADR 一覧

### Core Infrastructure

#### [ADR-001: Deferred Plugin Registration Pattern](./adr-001-deferred-plugin-registration-pattern.md)

**Status**: ✅ Accepted  
**概要**: Semantic Kernel プラグインのインスタンス化と DI 解決を Kernel のビルド時まで遅延させることで、クリーンな依存関係管理を実現します。  
**キーワード**: Plugin, Dependency Injection, Deferred Execution

---

#### [ADR-002: Provider-Specific Error Mapping Strategy](./adr-002-provider-specific-error-mapping-strategy.md)

**Status**: ✅ Accepted  
**概要**: 各 AI プロバイダーがスローする独自の例外を共通の VKError 形式に変換し、抽象化の純粋性とエラーハンドリングの一貫性を維持します。  
**キーワード**: Error Mapping, Abstraction, Result Pattern

---

#### [ADR-003: Scoped Kernel Lifecycle Management](./adr-003-scoped-kernel-lifecycle-management.md)

**Status**: ✅ Accepted  
**概要**: Kernel インスタンスとプラグインの生存期間をリクエストスコープに一致させることで、スレッド安全性とコンテキスト共有のバランスを最適化します。  
**キーワード**: Lifecycle, Scoped, Dependency Injection

---

#### [ADR-004: Semantic Memory Abstraction & Integration](./adr-004-semantic-memory-abstraction-integration.md)

**Status**: ✅ Accepted  
**概要**: Semantic Kernel のメモリ機能（ベクター検索等）を VK.Blocks 共通のインターフェースを通じて公開し、ベンダーに依存しないナレッジ検索（RAG）を実現します。  
**キーワード**: RAG, Vector Search, Semantic Memory

---

### Industrial DNA & Governance

#### [ADR-005: Industrial Encapsulation and Visibility Standard](./adr-005-industrial-encapsulation-and-visibility-standard.md)

**Status**: ✅ Accepted  
**概要**: 内部実装を `Internal/` サブディレクトリに隔離し、パブリック API 表面積を最小化することで、保守性と安全性を向上させます。  
**キーワード**: Encapsulation, Visibility, AP.03

---

#### [ADR-006: Semantic Retrieval Alignment](./adr-006-semantic-retrieval-alignment.md)

**Status**: ✅ Accepted  
**概要**: `Memory` 用語を `Retrieval` に統一し、基盤となる `AI` ブロックおよび現代的な RAG パターンとの整合性を確保します。  
**キーワード**: Retrieval, RAG, Terminology Alignment

---

#### [ADR-007: Unified AI Engine Base Pattern](./adr-007-unified-ai-engine-base-pattern.md)

**Status**: ✅ Accepted  
**概要**: `AISKEngineBase<T>` を導入し、接続解決、ガバナンス適用、エラーハンドリングの共通ロジックを全エンジンで一元化します。  
**キーワード**: DRY, Base Class, Governance

---

## 🎯 ADR の読み方ガイド

### プラグインシステムと拡張性の理解用
1. **ADR-001**: 複雑な依存関係を持つプラグインをどのように安全に DI コンテナに統合しているかを理解するために読んでください。

### エラーハンドリングと堅牢性の理解用
1. **ADR-002**: プロバイダー固有の失敗をどのように隠蔽し、アプリケーション側で扱いやすいエラー形式に統一しているかを理解するために読んでください。
2. **ADR-007**: 全エンジンで共通化されたエラーハンドリングと実行パイプラインの設計思想を理解するために読んでください。

### インフラストラクチャとライフサイクルの理解用
1. **ADR-003**: AI エンジンのリソース管理と、ユーザーコンテキストを安全に伝随させるための設計方針を理解するために読んでください。
2. **ADR-005**: 内部実装をどのように隠蔽し、クリーンなパブリック API を維持しているかを理解するために読んでください。

### ナレッジ検索と Retrieval の理解用
1. **ADR-004**: ベクターデータベースや検索機能をどのように抽象化し、プロバイダーを差し替え可能にしているかを理解するために読んでください。
2. **ADR-006**: なぜ `Memory` から `Retrieval` へ用語変更が行われたかのアーキテクチャ的意図を理解するために読んでください。

## 🔗 関連ドキュメント
- [AI.SemanticKernel Module Manifest](../../../src/BuildingBlocks/AI.SemanticKernel/module-manifest.md)

**Last Updated**: 2026-05-10  
**Total ADRs**: 7
