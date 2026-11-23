# Architecture Decision Records (ADR) - Index

こ�EチE��レクトリには、VK.Blocks.AI モジュールの主要な設計決定を記録した ADR が含まれてぁE��す、E

## 📚 ADR 一覧

### Core Architecture

#### [ADR-001: Provider-Agnostic Engine Abstraction](./adr-001-provider-agnostic-engine-abstraction.md)

**Status**: ✁EAccepted  
**概要E*: LLM プロバイダーめE��ーケストレーター SDK�E�Eemantic Kernel 等）への直接依存を排除し、封E��の技術変更に強ぁE��象匁EEngine 層を定義します、E 
**キーワーチE*: Vendor Neutral, Portability, Result Pattern

---

#### [ADR-002: Polymorphic Multi-modal Message Schema](./adr-002-polymorphic-multi-modal-message-schema.md)

**Status**: ✁EAccepted  
**概要E*: チE��スト、画像、E��声、ファイルなどの多様なコンチE��チE��統一皁E��扱ぁE��め�E、拡張可能なポリモーフィチE��なメチE��ージ構造を定義します、E 
**キーワーチE*: Multi-modal, Polymorphism, Content Schema

---

#### [ADR-003: Hierarchical Configuration Pattern](./adr-003-hierarchical-configuration-pattern.md)

**Status**: ✁EAccepted  
**概要E*: 褁E��の機�Eを�E匁E��めEAI モジュールにおいて、命名衝突を防ぎ可読性を高めるため�E階層皁E��設定管琁E��造を定義します、E 
**キーワーチE*: Options Pattern, Configuration, Hierarchical

---

#### [ADR-004: Standardized Execution Arguments (Args Pattern)](./adr-004-standardized-execution-arguments-args-pattern.md)

**Status**: ✁EAccepted  
**概要E*: リクエスト単位での挙動制御�E�Eemperature 等）を安�Eかつ拡張可能な方法で実現するための、Args レコードを用ぁE��オーバ�Eライドパターンを定義します、E 
**キーワーチE*: Args Pattern, Overrides, Immutable

---

#### [ADR-005: AI Options Architecture: Interface Segregation over Class Inheritance](./adr-005-ai-options-architecture-interface-segregation-over-class-inheritance.md)

**Status**: ✁EAccepted  
**概要E*: Options の設計をクラス継承からインターフェースベ�Eスの絁E��せへと転換し、ISP に準拠した柔軟で明示皁E��設定構造を定義します、E 
**キーワーチE*: Interface Segregation, Composition, Explicit Over Implicit

---

#### [ADR-015: Rename IVKxxxSettings interfaces to IVKxxxOptions in AI module](./adr-015-rename-ivkxxxsettings-interfaces-to-ivkxxxoptions-in-ai-module.md)

**Status**: ✁EAccepted  
**概要E*: AIモジュール冁E�Eすべての設定インターフェース�E�EVKxxxSettings�E�を統一皁E�� Options 命名規則�E�EVKxxxOptions�E�へとリネ�Eムし、アーキチE��チャ標準に完�E整合させます、E 
**キーワーチE*: Options Suffix, Naming Convention, Symmetry

---

#### [ADR-017: Extraction of Vectorics Abstractions to Standalone Modules](./adr-017-extraction-of-vectorics-abstractions-to-standalone-modules.md)

**Status**: ✁EAccepted  
**概要E*: ベクトル空間インチE��シングと生�E推論�E責務をクリーンに刁E��するため、埋め込み生�E、リランキング、RAG、セマンチE��チE��キャチE��ュとぁE��ぁEVectorics 関連機�E群めE`VK.Blocks.AI` から廁E��し、独立しぁEBuilding Block�E�EVectorStore`, `VectorSearch`�E�へと再編成する設計、E 
**キーワーチE*: Code Decoupling, Assembly Separation, Vectorics Demarcation

---

#### [ADR-018: Context Expansion Architecture for Context-Aware Prompt Enrichment](./adr-018-context-expansion-architecture-for-context-aware-prompt-enrichment.md)

**Status**: ✁EAccepted  
**概要E*: シスチE��プロンプトめE��ーザー持E��のコンチE��ストを安�Eかつ動的に拡張するため、不変なオプション設計に基づぁE`IVKContextExpansionStrategy` と標準化された実行スチE�Eジ `DefaultContextExpansionStage` を導�Eする設計、E 
**キーワーチE*: Context Expansion, Strategy Pattern, Prompt Enrichment

---

### Observability & Cost Control

#### [ADR-006: Unified Token Usage Tracking and Cost Observation](./adr-006-unified-token-usage-tracking-and-cost-observation.md)

**Status**: 📝 Draft  
**概要E*: プロバイダー間で異なるトークン計数方式を統一し、コスト�E透�E性と予算�Eースの制御�E�サーキチE��ブレーカー�E�を実現するための基盤を定義します、E 
**キーワーチE*: Token Usage, Cost Control, Circuit Breaker

---

#### [ADR-016: Standardized Observability Diagnostics Constants](./adr-016-standardized-observability-diagnostics-constants.md)

**Status**: ✁EAccepted  
**概要E*: 各AIサブ機�EにオフセチE��ベ�Eスの一意イベンチEDと標準化診断定数クラスを導�Eし、ログめE��トリクスの競合を防ぎつつ一貫した監視を可能にする可観測性設計、E 
**キーワーチE*: Event ID Offset, OpenTelemetry, LoggerMessage

---

### Logic & Protocol

#### [ADR-007: Standardized Streaming Protocol with Result Pattern](./adr-007-standardized-streaming-protocol-with-result-pattern.md)

**Status**: 📝 Draft  
**概要E*: IAsyncEnumerable を用ぁE��ストリーミングにおいて、エラーハンドリング�E�Eesult パターン�E�とメタチE�Eタ雁E��を一貫させるため�Eプロトコルを定義します、E 
**キーワーチE*: Streaming, IAsyncEnumerable, Error Handling

---

#### [ADR-013: Industrialization of AI Engine Infrastructure and Streaming Resilience](./adr-013-industrialization-of-ai-engine-infrastructure-and-streaming-resilience.md)

**Status**: ✁EAccepted  
**概要E*: 推琁E��チE���E�EoT�E�への対応、統一エラーマッピング、およ�E Channel を用ぁE��高性能な非ブロチE��ング流式�Eロトコルを定義します、E 
**キーワーチE*: Reasoning Support, Channels, Error Mapping

---

### Governance & Security

#### [ADR-008: Integrated Moderation and Governance Middleware](./adr-008-integrated-moderation-and-governance-middleware.md)

**Status**: 📝 Draft  
**概要E*: 入出力�E安�E性�E�不適刁E��ンチE��チE�E遮断�E�を、Decorator パターンを用ぁE��エンジンから独立して自動的に適用するガバナンス機構を定義します、E 
**キーワーチE*: Governance, Content Filter, Decorator Pattern

---

#### [ADR-014: Standardize AI Overrides Inheritance to IVKAIGovernanceOverrides](./adr-014-standardize-ai-overrides-inheritance-to-ivkaigovernanceoverrides.md)

**Status**: ✁EAccepted  
**概要E*: 吁E��ブ機�Eのオーバ�Eライド用インターフェースが統合ガバナンス�E�EVKAIGovernanceOverrides�E�を継承するように統一し、安�EフィルターめE��査ログのリクエストレベルでの動的トグルを完�E補完します、E 
**キーワーチE*: Overrides, Symmetry, AP.05 Contract

---

### Advanced Features (RAG & Resiliency)

#### [ADR-009: Retrieval-Augmented Generation (RAG) Interface Strategy](<./adr-009-retrieval-augmented-generation-(rag)-interface-strategy.md>)

**Status**: 📝 Draft  
**概要E*: 検索�E�Eetrieval�E�と生�E�E�Eeneration�E�をクリーンに刁E��し、特定�Eベクトル DB に依存しなぁE�Eータブルな RAG 実裁E�E持E�Eを定義します、E 
**キーワーチE*: RAG, Retrieval Engine, Knowledge Injection

---

#### [ADR-010: Multi-Engine Failover and Load Balancing](./adr-010-multi-engine-failover-and-load-balancing.md)

**Status**: 📝 Draft  
**概要E*: 褁E��の AI プロバイダーを束ね、レート制限やサービス停止時に自動的にフェイルオーバ�E�E�また�E負荷刁E���E�を行うためのレジリエンス戦略を定義します、E 
**キーワーチE*: High Availability, Failover, Load Balancing

---

### Lifecycle & QA

#### [ADR-011: Standardized Prompt Templating and Versioning](./adr-011-standardized-prompt-templating-and-versioning.md)

**Status**: 📝 Draft  
**概要E*: プロンプトのハ�Eドコードを排除し、Liquid チE��プレートと外部プロバイダーを用ぁE��ことで、デプロイなしでの挙動更新とバ�Eジョン管琁E��実現します、E 
**キーワーチE*: Prompt Engineering, Templating, Versioning

---

#### [ADR-012: Automated AI Evaluation Framework](./adr-012-automated-ai-evaluation-framework.md)

**Status**: 📝 Draft  
**概要E*: 非決定的な AI の出力を定量皁E��評価するための「LLM-as-a-Judge」パターンと、CI/CD における品質ゲート�E構築方針を定義します、E 
**キーワーチE*: QA, Evaluation, LLM-as-a-Judge

---

### Migration & Modernization (2026+)

#### [ADR-019: Migration from Semantic Kernel to Microsoft.Extensions.AI and Microsoft Agent Framework](./adr-019-migration-from-semantic-kernel-to-meai-and-maf.md)

**Status**: ✁EAccepted  
**概要E*: Semantic Kernel の技術的負債を一掁E��、ENET 標準�E Microsoft.Extensions.AI (MEAI) および Microsoft Agent Framework (MAF) へ全面皁E��移行する方針を定義、E 
**キーワーチE*: MEAI, MAF, DelegatingChatClient, Breaking Change

---

#### [ADR-020: Pure Isolation of VK.Blocks.AI Core Ports (Option B)](./adr-020-pure-isolation-of-vk-blocks-ai-core-ports.md)

**Status**: ✁EAccepted  
**概要E*: VK.Blocks.AI (Core) の純粋性を保護するため、外部 AI SDK (MEAI Abstractions 含む) への依存を排除し、独自の完�Eな Port-and-Adapter 墁E��を確立する、E 
**キーワーチE*: Pure Ports, Zero External Dependency, ARCH-03, Result Pattern

---

#### [ADR-021: Deterministic MEAI Middleware Pipeline](./adr-021-deterministic-meai-middleware-pipeline.md)

**Status**: ✁EAccepted  
**概要E*: MEAI DelegatingChatClient による7段階�E決定論的ミドルウェア実行頁E��（監視、セキュリチE��、�Eライバシー、トークンガバナンス、キャチE��ュ、ツール実行）を確立し、E��チE��スチEAIContent の完�E保持とレジリエンチEGoogle 連携を保証する、E 
**キーワーチE*: MEAI Middleware, DelegatingChatClient, Deterministic Pipeline, ARCH-05, ARCH-12, OR.03

---

#### [ADR-022: Compile-Time AIFunction Generation and Multi-Agent Workflows](./adr-022-compile-time-aifunction-and-agent-workflows.md)

**Status**: ✁EAccepted  
**概要E*: [VKAITool] 属性による Roslyn Source Generator ベ�Eスのゼロリフレクション IVKAtomicTool 自動生成、ToolExecutionContextAccessor による非同期墁E��でのコンチE��スト伝播、およ�E Microsoft.Agents.AI.Workflows と IVKAgentThreadStore を用ぁE��エージェント協調方針を定義、E 
**キーワーチE*: VKAITool, Source Generator, Zero Reflection, AsyncLocal Context, Multi-Agent Workflows, AP.01, ARCH-09

---

#### [ADR-023: Agent Framework Boundaries, Guardrails, and MEAI Decoupling](./adr-023-agent-framework-boundaries-and-guardrails.md)

**Status**: ✁EAccepted  
**概要E*: `VK.Blocks.AI` への純粋エージェント契紁E��紁E��DI 経由の `IChatClient` 解決による `AI.MEAI` からの疎結合化、Eつの `[VKFeature]` スライス�E�ESingleAgent` / `MultiAgent` / `AgentWorkflow`�E�への再編、実行ガードレール、�E通ツール承認パイプライン、およ�EチェチE��ポイント対応�E墁E��設計を定義、E 
**キーワーチE*: Agent Boundaries, MEAI Decoupling, Guardrails, Handoff, MAF Workflows, Checkpointing, ARCH-01, AP.06

---

## 🎯 ADR の読み方ガイチE

### アーキチE��チャとポ�EタビリチE��の琁E��用

1. **ADR-001**: なぁEAI ライブラリにおいてプロバイダー抽象化が不可欠なのか、その背景とトレードオフを琁E��するために最初に読んでください、E
2. **ADR-005**: 柔軟な設定管琁E��実現するための「継承から絁E��せへ」�E転換理由と、その実裁E��針を琁E��するために読んでください、E
3. **ADR-015**: 設定インターフェース命名�EOptionsサフィチE��ス標準化につぁE��琁E��するために読んでください、E
4. **ADR-017**: ベクトルインチE��シングめE��索の技術と、LLM生�Eのオーケストレーションを物琁E��センブリ・名前空間レベルで疎結合にする意義を学ぶために読んでください、E
5. **ADR-018**: プロンプトチE��プレートを静的・動的ナレチE��で安�Eに拡張するための、Context Expansion 構造とト�Eクン管琁E��つぁE��琁E��するために読んでください、E

### 運用、コスト、レジリエンスの琁E��用

1. **ADR-006**: 産業レベルの AI 運用に不可欠なト�Eクン管琁E��コスト制御の老E��方を理解するために読んでください、E
2. **ADR-010**: クラウド障害に強ぁE��褁E��のプロバイダーを絁E��合わせた高可用な設計を琁E��するために読んでください、E
3. **ADR-016**: 褁E��の AI サブ機�Eにまたがるトレース、ログ ID、メトリクス名を衝突なく設計し、統一ダチE��ュボ�Eドを構築する手法を琁E��するために読んでください、E

### 品質管琁E��ガバナンスの琁E��用

1. **ADR-008**: 企業レベルで忁E��となる安�E性�E�検閲�E��E自動化とガバナンス戦略につぁE��琁E��するために読んでください、E
2. **ADR-012**: 非決定的な AI シスチE��の品質をどのように「数値」で保証し、継続的チE��バリーを実現するかを琁E��するために読んでください、E
3. **ADR-014**: 静的解析による安�Eなオプション制御�E�EP.05�E�と、Settings側とOverrides側の美しぁE��称設計につぁE��琁E��するために読んでください、E

## 🔗 関連ドキュメンチE

- [AI Module Manifest](/src/BuildingBlocks/AI/.prompts/module-manifest.md)
- [AI.AgentFramework Module Manifest](/src/BuildingBlocks/AI.AgentFramework/.prompts/module-manifest.md)

**Last Updated**: 2026-11-15  
**Total ADRs**: 23
