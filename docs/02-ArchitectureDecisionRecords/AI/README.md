# Architecture Decision Records (ADR) - Index

このディレクトリには、VK.Blocks.AI モジュールの主要な設計決定を記録した ADR が含まれています。

## 📚 ADR 一覧

### Core Architecture

#### [ADR-001: Provider-Agnostic Engine Abstraction](./adr-001-provider-agnostic-engine-abstraction.md)

**Status**: ✅ Accepted  
**概要**: LLM プロバイダーやオーケストレーター SDK（Semantic Kernel 等）への直接依存を排除し、将来の技術変更に強い抽象化 Engine 層を定義します。  
**キーワード**: Vendor Neutral, Portability, Result Pattern

---

#### [ADR-002: Polymorphic Multi-modal Message Schema](./adr-002-polymorphic-multi-modal-message-schema.md)

**Status**: ✅ Accepted  
**概要**: テキスト、画像、音声、ファイルなどの多様なコンテンツを統一的に扱うための、拡張可能なポリモーフィックなメッセージ構造を定義します。  
**キーワード**: Multi-modal, Polymorphism, Content Schema

---

#### [ADR-003: Hierarchical Configuration Pattern](./adr-003-hierarchical-configuration-pattern.md)

**Status**: ✅ Accepted  
**概要**: 複数の機能を内包する AI モジュールにおいて、命名衝突を防ぎ可読性を高めるための階層的な設定管理構造を定義します。  
**キーワード**: Options Pattern, Configuration, Hierarchical

---

#### [ADR-004: Standardized Execution Arguments (Args Pattern)](./adr-004-standardized-execution-arguments-args-pattern.md)

**Status**: ✅ Accepted  
**概要**: リクエスト単位での挙動制御（Temperature 等）を安全かつ拡張可能な方法で実現するための、Args レコードを用いたオーバーライドパターンを定義します。  
**キーワード**: Args Pattern, Overrides, Immutable

---

#### [ADR-005: AI Options Architecture: Interface Segregation over Class Inheritance](./adr-005-ai-options-architecture-interface-segregation-over-class-inheritance.md)

**Status**: ✅ Accepted  
**概要**: Options の設計をクラス継承からインターフェースベースの組合せへと転換し、ISP に準拠した柔軟で明示的な設定構造を定義します。  
**キーワード**: Interface Segregation, Composition, Explicit Over Implicit

---

### Observability & Cost Control

#### [ADR-006: Unified Token Usage Tracking and Cost Observation](./adr-006-unified-token-usage-tracking-and-cost-observation.md)

**Status**: 📝 Draft  
**概要**: プロバイダー間で異なるトークン計数方式を統一し、コストの透明性と予算ベースの制御（サーキットブレーカー）を実現するための基盤を定義します。  
**キーワード**: Token Usage, Cost Control, Circuit Breaker

---

### Logic & Protocol

#### [ADR-007: Standardized Streaming Protocol with Result Pattern](./adr-007-standardized-streaming-protocol-with-result-pattern.md)

**Status**: 📝 Draft  
**概要**: IAsyncEnumerable を用いたストリーミングにおいて、エラーハンドリング（Result パターン）とメタデータ集計を一貫させるためのプロトコルを定義します。  
**キーワード**: Streaming, IAsyncEnumerable, Error Handling

---

### Governance & Security

#### [ADR-008: Integrated Moderation and Governance Middleware](./adr-008-integrated-moderation-and-governance-middleware.md)

**Status**: 📝 Draft  
**概要**: 入出力の安全性（不適切コンテンツの遮断）を、Decorator パターンを用いてエンジンから独立して自動的に適用するガバナンス機構を定義します。  
**キーワード**: Governance, Content Filter, Decorator Pattern

---

### Advanced Features (RAG & Resiliency)

#### [ADR-009: Retrieval-Augmented Generation (RAG) Interface Strategy](./adr-009-retrieval-augmented-generation-(rag)-interface-strategy.md)

**Status**: 📝 Draft  
**概要**: 検索（Retrieval）と生成（Generation）をクリーンに分離し、特定のベクトル DB に依存しないポータブルな RAG 実装の指針を定義します。  
**キーワード**: RAG, Retrieval Engine, Knowledge Injection

---

#### [ADR-010: Multi-Engine Failover and Load Balancing](./adr-010-multi-engine-failover-and-load-balancing.md)

**Status**: 📝 Draft  
**概要**: 複数の AI プロバイダーを束ね、レート制限やサービス停止時に自動的にフェイルオーバー（または負荷分散）を行うためのレジリエンス戦略を定義します。  
**キーワード**: High Availability, Failover, Load Balancing

---

### Lifecycle & QA

#### [ADR-011: Standardized Prompt Templating and Versioning](./adr-011-standardized-prompt-templating-and-versioning.md)

**Status**: 📝 Draft  
**概要**: プロンプトのハードコードを排除し、Liquid テンプレートと外部プロバイダーを用いることで、デプロイなしでの挙動更新とバージョン管理を実現します。  
**キーワード**: Prompt Engineering, Templating, Versioning

---

#### [ADR-012: Automated AI Evaluation Framework](./adr-012-automated-ai-evaluation-framework.md)

**Status**: 📝 Draft  
**概要**: 非決定的な AI の出力を定量的に評価するための「LLM-as-a-Judge」パターンと、CI/CD における品質ゲートの構築方針を定義します。  
**キーワード**: QA, Evaluation, LLM-as-a-Judge

---

## 🎯 ADR の読み方ガイド

### アーキテクチャとポータビリティの理解用
1. **ADR-001**: なぜ AI ライブラリにおいてプロバイダー抽象化が不可欠なのか、その背景とトレードオフを理解するために最初に読んでください。
2. **ADR-005**: 柔軟な設定管理を実現するための「継承から組合せへ」の転換理由と、その実装方針を理解するために読んでください。

### 運用、コスト、レジリエンスの理解用
1. **ADR-006**: 産業レベルの AI 運用に不可欠なトークン管理とコスト制御の考え方を理解するために読んでください。
2. **ADR-010**: クラウド障害に強い、複数のプロバイダーを組み合わせた高可用な設計を理解するために読んでください。

### 品質管理とガバナンスの理解用
1. **ADR-008**: 企業レベルで必須となる安全性（検閲）の自動化とガバナンス戦略について理解するために読んでください。
2. **ADR-012**: 非決定的な AI システムの品質をどのように「数値」で保証し、継続的デリバリーを実現するかを理解するために読んでください。

## 🔗 関連ドキュメント
- [AI Module Manifest](../../../src/BuildingBlocks/AI/module-manifest.md)

**Last Updated**: 2026-05-10  
**Total ADRs**: 12
