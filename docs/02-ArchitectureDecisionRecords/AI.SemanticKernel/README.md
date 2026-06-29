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

#### [ADR-008: Industrializing AI Semantic Kernel Architecture](./adr-008-industrializing-ai-semantic-kernel-architecture.md)

**Status**: ✅ Accepted  
**概要**: 基础设施のリークを排除するためにファクトリを内部化し、ネイティブなカーネルキャッシュ装飾と TimeProvider による決定的な実行を導入します。  
**キーワード**: Industrialization, Caching, Determinism, AP.03, CS.06

---

#### [ADR-009: Applying Cross Cutting Concerns via Semantic Kernel Filters](./adr-009-applying-cross-cutting-concerns-via-semantic-kernel-filters.md)

**Status**: ✅ Accepted  
**概要**: レート制限、トークン監査、PII脱敏などを各エンジンに重複実装するのを避け、SKの `IPromptRenderFilter` や `IFunctionInvocationFilter` などのフィルター機能を用いて一元的にインターセプトする設計。  
**キーワード**: Cross-Cutting Concerns, SK Filters, Security Interceptor

---

#### [ADR-010: Provider Agnostic Multi Agent Collaboration](./adr-010-provider-agnostic-multi-agent-collaboration.md)

**Status**: ✅ Accepted  
**概要**: ベンダー固有のエージェントAPIからアプリケーションコードを切り離し、コアの `IVKAgent` 抽象と SK 独自の `AgentGroupChat` 駆動によるマルチエージェント協調・動的ルーティング・キーワード熔断を可能にする設計。  
**キーワード**: Multi-Agent, AgentGroupChat, Dynamic Routing

---

#### [ADR-011: Unified Vector ReRanking Engine Integration](./adr-011-unified-vector-reranking-engine-integration.md)

**Status**: ✅ Accepted  
**概要**: ベクトル類似度による召回結果のノイズ混入とトークン浪費を防ぐため、初篩結果に対し Cross-Encoder 重配（ReRank）フェーズを実行する `IVKReRankerEngine` インターフェースと統合実装の定義。  
**キーワード**: Vector ReRanking, RAG Optimization, Precision Boost

---

#### [ADR-012: Semantic Cache Integration](./adr-012-semantic-cache-integration.md)

**Status**: ✅ Accepted  
**概要**: 自然言語クエリの表記揺れによる従来の厳格一致キャッシュの限界を打破し、ベクトル検索のコサイン類似度としきい値判定を用いた、応答の高速バイパスセマンティックキャッシュの統合。  
**キーワード**: Semantic Cache, Vector Similarity, Cost Reduction

---

#### [ADR-013: Multi Provider Resilience Failover via Composite Chat Completion Service](./adr-013-multi-provider-resilience-failover-via-composite-chat-completion-service.md)

**Status**: ✅ Accepted  
**概要**: LLM呼び出しにおける一時的なレート制限（HTTP 429）や接続障害を検知し、Polly v8 レジリエンスパイプラインを介して自動的にバックアッププロバイダーや代替モデルIDへフェイルオーバーする複合サービス設計。  
**キーワード**: Composite Service, Resilience Failover, Polly v8, High Availability

---

## 🎯 ADR の読み方ガイド

### プラグインシステムと拡張性の理解用

1. **ADR-001**: 複雑な依存関係を持つプラグインをどのように安全に DI コンテナに統合しているかを理解するために読んでください。

### エラーハンドリングと安全・監査の理解用

1. **ADR-002**: プロバイダー固有の失敗をどのように隠蔽し、アプリケーション側で扱いやすいエラー形式に統一しているかを理解するために読んでください。
2. **ADR-007**: 全エンジンで共通化されたエラーハンドリングと実行パイプラインの設計思想を理解するために読んでください。
3. **ADR-009**: レート制限やPIIマスク、プロンプトインジェクションといった防線をどのように一元化して適用しているかを理解するために読んでください。

### インフラストラクチャとライフサイクルの理解用

1. **ADR-003**: AI エンジンのリソース管理と、ユーザーコンテキストを安全に伝随させるための設計方針を理解するために読んでください。
2. **ADR-005**: 内部実装をどのように隠蔽し、クリーンなパブリック API を維持しているかを理解するために読んでください。
3. **ADR-008**: 外部 SDK への依存を遮断し、高性能なキャッシュと決定的な実行環境をどのように構築しているかを理解するために読んでください。
4. **ADR-012**: 重複する高コストなLLM呼び出しをどのようにベクトル類似度で高速バイパスしているかを理解するために読んでください。
5. **ADR-013**: クラウド API のレート制限やダウンタイムに耐え、無停止で代替モデルへルーティングする高可用性フェイルオーバー設計を理解するために読んでください。

### ナレッジ検索と多エージェント協調の理解用

1. **ADR-004**: ベクターデータベースや検索機能をどのように抽象化し、プロバイダーを差し替え可能にしているかを理解するために読んでください。
2. **ADR-006**: なぜ `Memory` から `Retrieval` へ用語変更が行われたかのアーキテクチャ的意図を理解するために読んでください。
3. **ADR-010**: 自律的な役割分担を持つ複数AIエージェントの動的ルーティングと協調フローがどのように実現されているかを理解するために読んでください。
4. **ADR-011**: 初期の粗い召回ドキュメントをどのように二次スコアリングしてプロンプトサイズを絞っているかを理解するために読んでください。

## 🔗 関連ドキュメント

- [AI.SemanticKernel Module Manifest](../../../src/BuildingBlocks/AI.SemanticKernel/module-manifest.md)

**Last Updated**: 2026-06-07  
**Total ADRs**: 13
